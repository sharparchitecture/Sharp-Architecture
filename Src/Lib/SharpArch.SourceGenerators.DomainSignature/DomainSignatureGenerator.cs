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

        // Register a syntax provider that will look for classes with [GenerateSignature] attribute
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateSignatureAttributeFullName,
                static (s, _) => true,
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
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "SHARPARCH001",
                            "Class must be partial",
                            "Class '{0}' must be declared as partial to support signature generation",
                            "SharpArch",
                            DiagnosticSeverity.Error,
                            true),
                        null, //source.ClassDeclaration.GetLocation(),
                        source.ClassName));

                if (!source.IsBaseTypeCorrect)
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "SHARPARCH002",
                            "Class must derive from BaseObject",
                            "Class '{0}' must derive from BaseObject to support signature generation",
                            "SharpArch",
                            DiagnosticSeverity.Error,
                            true),
                        null, //source.ClassDeclaration.GetLocation(),
                        source.ClassName));
                if (source.DomainSignatureProperties.Count == 0)
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "SHARPARCH003",
                            "No DomainSignature properties found",
                            "Class '{0}' has no properties marked with DomainSignatureAttribute",
                            "SharpArch",
                            DiagnosticSeverity.Warning,
                            true),
                        null, //source.ClassDeclaration.GetLocation(),
                        source.ClassName));
                // Generate the source code
                var sourceCode = SourceCodeGenerator.Generate(source);
                // Add the source to the compilation
                spc.AddSource($"{source.ClassName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
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
            if (syntax is ClassDeclarationSyntax classDecl)
                if (classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
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
            isPartial, derivesFromBaseObject);
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
