namespace SharpArch.Domain.DomainModel;

/// <summary>
///     Apply source code generation to calculate hashcode and comparison based on <seealso cref="DomainSignatureAttribute" />.
/// </summary>
[Serializable]
[AttributeUsage(AttributeTargets.Class)]
[PublicAPI]
[BaseTypeRequired(typeof(IEntity<>))]
public sealed class GenerateDomainSignatureAttribute : Attribute
{
}
