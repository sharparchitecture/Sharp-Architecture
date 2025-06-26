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
    ///     Gets a value indicating whether the entity class inherits from the correct base type.
    /// </summary>
    public readonly bool IsBaseTypeCorrect;

    /// <summary>
    ///     Gets a value indicating whether the entity class is declared as partial.
    /// </summary>
    public readonly bool IsPartial;

    /// <summary>
    ///     Gets the namespace of the entity class.
    /// </summary>
    public readonly string ClassNamespace;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityInfo" /> struct.
    /// </summary>
    /// <param name="classNamespace">The namespace of the entity class.</param>
    /// <param name="className">The name of the entity class.</param>
    /// <param name="domainSignatureProperties">The properties that make up the domain signature.</param>
    /// <param name="isPartial">Whether the entity class is declared as partial.</param>
    /// <param name="isBaseTypeCorrect">Whether the entity class inherits from the correct base type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domainSignatureProperties" /> is null.</exception>
    public EntityInfo(
        string classNamespace, string className, List<DomainSignaturePropertyInfo> domainSignatureProperties, bool isPartial, bool isBaseTypeCorrect)
    {
        if (domainSignatureProperties == null)
            throw new ArgumentNullException(nameof(domainSignatureProperties));

        ClassNamespace = classNamespace;
        ClassName = className;
        IsPartial = isPartial;
        IsBaseTypeCorrect = isBaseTypeCorrect;
        DomainSignatureProperties = new EquatableArray<DomainSignaturePropertyInfo>(domainSignatureProperties.ToArray());
    }
}
