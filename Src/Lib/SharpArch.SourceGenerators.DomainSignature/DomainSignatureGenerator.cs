// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - don't use LINQ here to avoid unnecessary allocations/performance overhead

namespace SharpArch.SourceGenerators.DomainSignature;

using System.Text;
using Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


/// <summary>
///     Source generator that automatically implements domain signature methods for entity classes.
///     This generator creates GetHashCode and HasSameObjectSignatureAs implementations
///     for entity classes decorated with the GenerateSignatureAttribute.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class DomainSignatureGenerator : IIncrementalGenerator
{
    const string DomainSignatureAttributeShortName = "DomainSignature";
    const string DomainSignatureAttributeName = $"{DomainSignatureAttributeShortName}Attribute";
    const string BaseObjectTypeName = "BaseObject";
    const string GenerateSignatureAttributeFullName = "SharpArch.Domain.DomainModel.GenerateDomainSignatureAttribute";

    static readonly DiagnosticDescriptor MustBePartialDescriptor = new(
        "SHARPARCH001",
        "Class must be partial",
        "Class '{0}' must be declared as partial to support signature generation",
        "SharpArch",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor MustDeriveFromBaseObjectDescriptor = new(
        "SHARPARCH002",
        "Class must derive from BaseObject",
        "Class '{0}' must derive from BaseObject to support signature generation",
        "SharpArch",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor NoDomainSignaturePropertiesDescriptor = new(
        "SHARPARCH003",
        "No DomainSignature properties found",
        "Class '{0}' has no properties marked with DomainSignatureAttribute",
        "SharpArch",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor MustBeInNamespaceDescriptor = new(
        "SHARPARCH004",
        "Class must be declared in a namespace",
        "Class '{0}' must be declared in a namespace to support signature generation",
        "SharpArch",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor MustNotBeNestedDescriptor = new(
        "SHARPARCH005",
        "Class must not be nested",
        "Class '{0}' must not be nested to support signature generation",
        "SharpArch",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <inheritdoc />
    /// <summary>
    ///     Initializes the source generator with the given context.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("DomainSignatureComparerHelper.g.cs",
                SourceText.From(SourceCodeGeneratorHelper.DomainSignatureComparerHelper, Encoding.UTF8));
        });

        // Register a syntax provider that will look for types with [GenerateSignature] attribute.
        // The predicate is a cheap, syntax-only pre-filter so the (more expensive) semantic
        // transform only runs for class/record declarations carrying the attribute.
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateSignatureAttributeFullName,
                static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (ctx, _) => GetEntityToProcess(ctx)
            ).WithTrackingName("InitialExtraction")
            .Where(static x => x is not null)
            .Select(static (x, _) => x!.Value)
            .WithTrackingName("RemovingNulls");

        // Register the source output
        context.RegisterSourceOutput(classDeclarations,
            (spc, source) =>
            {
                if (!source.IsPartial)
                    spc.ReportDiagnostic(Diagnostic.Create(MustBePartialDescriptor, null, source.ClassName));

                if (!source.IsBaseTypeCorrect)
                    spc.ReportDiagnostic(Diagnostic.Create(MustDeriveFromBaseObjectDescriptor, null, source.ClassName));

                if (!source.HasNamespace)
                    spc.ReportDiagnostic(Diagnostic.Create(MustBeInNamespaceDescriptor, null, source.ClassName));

                if (source.IsNested)
                    spc.ReportDiagnostic(Diagnostic.Create(MustNotBeNestedDescriptor, null, source.ClassName));

                if (source.DomainSignatureProperties.Count == 0)
                    spc.ReportDiagnostic(Diagnostic.Create(NoDomainSignaturePropertiesDescriptor, null, source.ClassName));

                // Only emit generated source when the entity is in a valid state.
                // Otherwise the generated partial would not compile (e.g. overriding a
                // non-existent member or adding a partial part to a non-partial type),
                // producing cascading compiler errors on top of the diagnostics above.
                if (!source.IsPartial || !source.IsBaseTypeCorrect || !source.HasNamespace || source.IsNested)
                    return;

                // Generate the source code
                var sourceCode = SourceCodeGenerator.Generate(source);
                // Include the namespace in the hint name so that two classes with the
                // same name in different namespaces do not collide (AddSource requires
                // globally-unique hint names).
                var hint = string.IsNullOrEmpty(source.ClassNamespace)
                    ? $"{source.ClassName}.g.cs"
                    : $"{source.ClassNamespace}.{source.ClassName}.g.cs";
                spc.AddSource(hint, SourceText.From(sourceCode, Encoding.UTF8));
            });
    }

    /// <summary>
    ///     Extracts entity information from the class decorated with GenerateSignatureAttribute.
    /// </summary>
    /// <param name="ctx">The generator attribute syntax context.</param>
    /// <returns>
    ///     An <see cref="EntityInfo" /> instance containing information about the entity class,
    ///     or null if the class cannot be processed.
    /// </returns>
    [Pure]
    static EntityInfo? GetEntityToProcess(GeneratorAttributeSyntaxContext ctx)
    {
        var classDeclaration = ctx.TargetNode;
        // Get the semantic model for this class declaration
        var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol == null)
            return null;

        var isPartial = false;
        foreach (var syntaxRef in classSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is TypeDeclarationSyntax typeDecl)
                if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    isPartial = true;
                    break;
                }
        }

        // Check if class derives from BaseObject - if not, emit error
        var derivesFromBaseObject = false;
        var currentType = classSymbol;
        while (currentType.BaseType != null)
        {
            if (currentType.BaseType.Name == BaseObjectTypeName)
            {
                derivesFromBaseObject = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        // Get all properties with [DomainSignature] attribute
        var domainSignatureProperties = GetDomainSignatureProperties(classSymbol);

        return new EntityInfo(classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            domainSignatureProperties,
            isPartial, derivesFromBaseObject,
            hasNamespace: !classSymbol.ContainingNamespace.IsGlobalNamespace,
            isNested: classSymbol.ContainingType != null,
            isRecord: classSymbol.IsRecord);
    }

    /// <summary>
    ///     Gets all properties marked with the DomainSignatureAttribute from the given class.
    /// </summary>
    /// <param name="classSymbol">The class symbol to analyze.</param>
    /// <returns>A list of properties that are part of the domain signature.</returns>
    static List<DomainSignaturePropertyInfo> GetDomainSignatureProperties(INamedTypeSymbol classSymbol)
    {
        var signatureProperties = new List<DomainSignaturePropertyInfo>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
                continue;

            if (propertySymbol.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name is DomainSignatureAttributeName or DomainSignatureAttributeShortName))
            {
                SignatureMemberType? memberType = null;

                if (propertySymbol.Type.IsReferenceType)
                    memberType = SignatureMemberType.Reference;
                else if (propertySymbol.Type.IsValueType && propertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
                    memberType = SignatureMemberType.NullableValue;
                else
                    memberType = SignatureMemberType.Value;

                if (memberType is null)
                    throw new InvalidOperationException(
                        $"Property '{propertySymbol.Name}' in class '{classSymbol.Name}' has an unsupported type for domain signature.");

                signatureProperties.Add(new DomainSignaturePropertyInfo(propertySymbol.Name, memberType.Value));
            }
        }

        return signatureProperties;
    }
}