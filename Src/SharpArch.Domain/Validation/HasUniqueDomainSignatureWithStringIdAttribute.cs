namespace SharpArch.Domain.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using JetBrains.Annotations;
    using SharpArch.Domain.DomainModel;

    /// <summary>
    ///     Performs validation of domain signature uniqueness.
    /// </summary>
    /// <remarks>
    ///     Performs database search to ensure no other entity with same domain signature exists.
    /// </remarks>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    /// <seealso cref="DomainSignatureAttribute" />
    /// .
    /// <seealso cref="HasUniqueDomainSignatureAttribute" />
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    [BaseTypeRequired(typeof(IEntityWithTypedId<string>))]
    public sealed class HasUniqueDomainSignatureWithStringIdAttribute : HasUniqueDomainSignatureAttributeBase
    {
        /// <summary>
        ///     Performs validation.
        /// </summary>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return DoValidate<string>(value, validationContext);
        }
    }
}
