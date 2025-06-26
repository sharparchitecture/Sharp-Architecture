namespace SharpArch.SourceGenerators.DomainSignature.Impl;

/// <summary>
///     Describes the type of property in an entity's domain signature.
/// </summary>
public enum SignatureMemberType : byte
{
    /// <summary>
    ///     Value type.
    /// </summary>
    Value,

    /// <summary>
    ///     Nullable value type.
    /// </summary>
    NullableValue,

    /// <summary>
    ///     Reference type.
    /// </summary>
    Reference
}


/// <summary>
///     Contains information about a property that is part of an entity's domain signature.
/// </summary>
public readonly record struct DomainSignaturePropertyInfo
{
    /// <summary>
    ///     Gets the name of the property.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Gets the type of the property in the domain signature.
    /// </summary>
    public readonly SignatureMemberType MemberType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainSignaturePropertyInfo" /> struct.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="memberType">Type of the property.</param>
    public DomainSignaturePropertyInfo(string name, SignatureMemberType memberType)
    {
        Name = name;
        MemberType = memberType;
    }
}
