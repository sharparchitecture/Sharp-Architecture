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

    // NOTE: The message text is mirrored at runtime by
    // SharpArch.Domain.DomainModel.Entity.GetTypeSpecificSignatureProperties, which rejects the same
    // pointer and ref struct properties. Generic type parameters are rejected at compile time only.
    // Keep both sides in sync.
    static readonly DiagnosticDescriptor UnsupportedPropertyTypeDescriptor = new(
        "SHARPARCH006",
        "Property has an unsupported type for domain signature",
        "Property '{0}' in class '{1}' has an unsupported type for domain signature",
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
                if (source.Errors.HasFlag(GenerationError.NotPartial))
                    spc.ReportDiagnostic(Diagnostic.Create(MustBePartialDescriptor, null, source.ClassName));

                if (source.Errors.HasFlag(GenerationError.WrongBaseType))
                    spc.ReportDiagnostic(Diagnostic.Create(MustDeriveFromBaseObjectDescriptor, null, source.ClassName));

                if (source.Errors.HasFlag(GenerationError.NoNamespace))
                    spc.ReportDiagnostic(Diagnostic.Create(MustBeInNamespaceDescriptor, null, source.ClassName));

                if (source.Errors.HasFlag(GenerationError.Nested))
                    spc.ReportDiagnostic(Diagnostic.Create(MustNotBeNestedDescriptor, null, source.ClassName));

                foreach (var propertyName in source.UnsupportedPropertyNames)
                    spc.ReportDiagnostic(Diagnostic.Create(UnsupportedPropertyTypeDescriptor, null, propertyName, source.ClassName));

                // Only warn about a missing signature when no properties were rejected above,
                // otherwise SHARPARCH006 already explains why the signature looks empty.
                if (source.DomainSignatureProperties.Count == 0 && source.UnsupportedPropertyNames.Count == 0)
                    spc.ReportDiagnostic(Diagnostic.Create(NoDomainSignaturePropertiesDescriptor, null, source.ClassName));

                // Only emit generated source when the entity is in a valid state.
                // Otherwise the generated partial would not compile (e.g. overriding a
                // non-existent member or adding a partial part to a non-partial type),
                // producing cascading compiler errors on top of the diagnostics above.
                if (source.Errors != GenerationError.None || source.UnsupportedPropertyNames.Count > 0)
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

        // Collect validation errors as flags; the corresponding diagnostics are reported
        // from the source output callback.
        var errors = GenerationError.None;
        if (!isPartial)
            errors |= GenerationError.NotPartial;
        if (!derivesFromBaseObject)
            errors |= GenerationError.WrongBaseType;
        if (classSymbol.ContainingNamespace.IsGlobalNamespace)
            errors |= GenerationError.NoNamespace;
        if (classSymbol.ContainingType != null)
            errors |= GenerationError.Nested;

        // Get all properties with [DomainSignature] attribute
        var (domainSignatureProperties, unsupportedPropertyNames) = GetDomainSignatureProperties(classSymbol);

        return new EntityInfo(classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            domainSignatureProperties,
            errors,
            classSymbol.IsRecord,
            unsupportedPropertyNames: new EquatableArray<string>(unsupportedPropertyNames.ToArray()));
    }

    /// <summary>
    ///     Gets all properties marked with the DomainSignatureAttribute from the given class.
    /// </summary>
    /// <param name="classSymbol">The class symbol to analyze.</param>
    /// <returns>
    ///     The properties that are part of the domain signature and the names of the properties whose
    ///     types are not supported as part of a domain signature.
    /// </returns>
    static (List<DomainSignaturePropertyInfo> SignatureProperties, List<string> UnsupportedPropertyNames)
        GetDomainSignatureProperties(INamedTypeSymbol classSymbol)
    {
        var signatureProperties = new List<DomainSignaturePropertyInfo>();
        var unsupportedPropertyNames = new List<string>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
                continue;

            if (propertySymbol.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name is DomainSignatureAttributeName or DomainSignatureAttributeShortName))
            {
                if (IsUnsupportedSignatureType(propertySymbol.Type))
                    unsupportedPropertyNames.Add(propertySymbol.Name);
                else
                    signatureProperties.Add(new DomainSignaturePropertyInfo(propertySymbol.Name,
                        GetSignatureMemberType(propertySymbol)));
            }
        }

        return (signatureProperties, unsupportedPropertyNames);
    }

    /// <summary>
    ///     Determines whether the property type is not supported as part of a domain signature:
    ///     pointer types, ref struct types and generic type parameters cannot be reliably hashed
    ///     or compared.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Pointer and ref struct properties are also rejected at runtime by
    ///         SharpArch.Domain.DomainModel.Entity.GetTypeSpecificSignatureProperties - keep both in sync.
    ///     </para>
    ///     <para>
    ///         Generic type parameters are a compile-time-only concern: the runtime only ever sees
    ///         closed instantiations, where type parameters are already substituted.
    ///     </para>
    /// </remarks>
    /// <param name="propertyType">The type of the property.</param>
    /// <returns>Whether the type is not supported as part of a domain signature.</returns>
    static bool IsUnsupportedSignatureType(ITypeSymbol propertyType)
        => propertyType.IsRefLikeType ||
           propertyType.TypeKind is TypeKind.Pointer or TypeKind.TypeParameter;

    /// <summary>
    ///     Determines the signature member type of a property based on its type and nullability:
    ///     reference types, nullable value types and non-nullable value types are compared differently.
    /// </summary>
    /// <param name="propertySymbol">The property to classify.</param>
    /// <returns>The signature member type of the property.</returns>
    static SignatureMemberType GetSignatureMemberType(IPropertySymbol propertySymbol)
        => propertySymbol switch
        {
            { Type.IsReferenceType: true }
                => SignatureMemberType.Reference,
            { Type.IsValueType: true, Type.NullableAnnotation: NullableAnnotation.Annotated }
                => SignatureMemberType.NullableValue,
            _ => SignatureMemberType.Value,
        };
}