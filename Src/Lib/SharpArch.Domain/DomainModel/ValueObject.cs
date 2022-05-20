namespace SharpArch.Domain.DomainModel;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;


/// <summary>
///     Provides a standard base class for facilitating comparison of value objects using all the object's properties.
/// </summary>
/// <remarks>
///     For a discussion of the implementation of Equals/GetHashCode, see
///     http://devlicio.us/blogs/billy_mccafferty/archive/2007/04/25/using-equals-gethashcode-effectively.aspx
///     and http://groups.google.com/group/sharp-architecture/browse_thread/thread/f76d1678e68e3ece?hl=en for
///     an in depth and conclusive resolution.
/// </remarks>
[Serializable]
[PublicAPI]
[SuppressMessage("ReSharper", "RedundantOverridenMember", Justification =
    "Need to override Equals/GetHashCode because  == and != operators were introduced")]
public abstract class ValueObject : BaseObject
{
    /// <summary>
    ///     Implements the <c>==</c> operator.
    /// </summary>
    /// <param name="valueObject1">The first value object.</param>
    /// <param name="valueObject2">The second value object.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(ValueObject? valueObject1, ValueObject? valueObject2)
    {
        if (ReferenceEquals(valueObject1, null)) return ReferenceEquals(valueObject2, null);

        return valueObject1.Equals(valueObject2);
    }

    /// <summary>
    ///     Implements the <c>!=</c> operator.
    /// </summary>
    /// <param name="valueObject1">The first value object.</param>
    /// <param name="valueObject2">The second value object.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(ValueObject? valueObject1, ValueObject? valueObject2)
        => !(valueObject1 == valueObject2);

    /// <summary>
    ///     Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object" /> to compare with the current <see cref="object" />.</param>
    /// <returns>
    ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    /// <remarks>
    ///     This is used to provide the hash code identifier of an object using the signature
    ///     properties of the object; although it's necessary for NHibernate's use, this can
    ///     also be useful for business logic purposes and has been included in this base
    ///     class, accordingly. Since it is recommended that GetHashCode change infrequently,
    ///     if at all, in an object's lifetime, it's important that properties are carefully
    ///     selected which truly represent the signature of an object.
    /// </remarks>
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    ///     Returns the signature properties that are specific to the type of the current object.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     ValueObject has properties marked with
    ///     <see cref="DomainSignatureAttribute" />
    /// </exception>
    protected override PropertyInfo[] GetTypeSpecificSignatureProperties()
    {
        var hasDomainSignature = GetType().GetProperties().Any(p => p.IsDefined(typeof(DomainSignatureAttribute), true));

        if (hasDomainSignature)
        {
            string message = "Properties were found within " + GetType() +
                @" having the
                [DomainSignature] attribute. The domain signature of a value object includes all
                of the properties of the object by convention; consequently, adding [DomainSignature]
                to the properties of a value object's properties is misleading and should be removed. 
                Alternatively, you can inherit from Entity if that fits your needs better.";

            throw new InvalidOperationException(message);
        }

        return GetType().GetProperties();
    }
}
