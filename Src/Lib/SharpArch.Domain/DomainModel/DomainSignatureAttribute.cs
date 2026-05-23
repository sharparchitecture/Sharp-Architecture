namespace SharpArch.Domain.DomainModel;

/// <summary>
///     Facilitates indicating which property(s) describe the unique signature of an
///     entity. See <see cref="BaseObject.GetTypeSpecificSignatureProperties" /> for when this is leveraged.
/// </summary>
/// <remarks>
///     This is intended for use with <see cref="Entity{TId}" />. It may NOT be used on a <see cref="ValueObject" />.
/// </remarks>
[Serializable]
[AttributeUsage(AttributeTargets.Property)]
[PublicAPI]
[BaseTypeRequired(typeof(IEntity<>))]
public sealed class DomainSignatureAttribute : Attribute
{
}
