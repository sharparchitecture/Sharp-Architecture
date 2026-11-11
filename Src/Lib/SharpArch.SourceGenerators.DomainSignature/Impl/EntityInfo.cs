// ReSharper disable UseArgumentNullExceptionThrow
namespace SharpArch.SourceGenerators.DomainSignature.Impl;

/// <summary>
///     Contains information about an entity class for which domain signature code will be generated.
///     This includes metadata about the class and its domain signature properties.
/// </summary>
public readonly record struct EntityInfo
{
    /// <summary>
    ///     Gets the name of the entity class.
    /// </summary>
    public readonly string ClassName;

    /// <summary>
    ///     Gets the collection of properties that make up the domain signature of the entity.
    /// </summary>
    public readonly EquatableArray<DomainSignaturePropertyInfo> DomainSignatureProperties;

    /// <summary>
    ///     Gets the reasons, if any, why the entity class is not in a valid state for signature generation.
    /// </summary>
    public readonly GenerationError Errors;

    /// <summary>
    ///     Gets a value indicating whether the entity class is a record.
    /// </summary>
    public readonly bool IsRecord;

    /// <summary>
    ///     Gets the namespace of the entity class.
    /// </summary>
    public readonly string ClassNamespace;

    /// <summary>
    ///     Gets the names of properties whose types are not supported as part of the domain signature.
    /// </summary>
    public readonly EquatableArray<string> UnsupportedPropertyNames;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityInfo" /> struct.
    /// </summary>
    /// <param name="classNamespace">The namespace of the entity class (empty for the global namespace).</param>
    /// <param name="className">The name of the entity class.</param>
    /// <param name="domainSignatureProperties">The properties that make up the domain signature.</param>
    /// <param name="errors">The reasons, if any, why the entity class is not in a valid state for signature generation.</param>
    /// <param name="isRecord">Whether the entity class is a record.</param>
    /// <param name="unsupportedPropertyNames">The names of properties whose types are not supported as part of the domain signature.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainSignatureProperties" /> is null.</exception>
    public EntityInfo(
        string classNamespace, string className, List<DomainSignaturePropertyInfo> domainSignatureProperties,
        GenerationError errors = GenerationError.None, bool isRecord = false,
        EquatableArray<string> unsupportedPropertyNames = default)
    {
        if (domainSignatureProperties == null)
            throw new ArgumentNullException(nameof(domainSignatureProperties));

        ClassNamespace = classNamespace;
        ClassName = className;
        Errors = errors;
        IsRecord = isRecord;
        UnsupportedPropertyNames = unsupportedPropertyNames;
        DomainSignatureProperties = new EquatableArray<DomainSignaturePropertyInfo>(domainSignatureProperties.ToArray());
    }
}