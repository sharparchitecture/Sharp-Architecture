namespace SharpArch.Domain.DomainModel
{
    using System;

    /// <summary>
    ///     Facilitates indicating which property(s) describe the unique signature of an 
    ///     entity.  See Entity.GetTypeSpecificSignatureProperties() for when this is leveraged.
    /// </summary>
    /// <remarks>
    ///     This is intended for use with <see cref = "Entity" />.  It may NOT be used on a <see cref = "ValueObject" />.
    /// </remarks>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DomainSignatureAttribute : Attribute
    {
    }
}