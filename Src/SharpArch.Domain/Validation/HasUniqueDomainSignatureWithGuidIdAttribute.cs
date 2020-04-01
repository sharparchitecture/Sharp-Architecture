namespace SharpArch.Domain.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using JetBrains.Annotations;
    using SharpArch.Domain.DomainModel;

    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    [BaseTypeRequired(typeof(IEntityWithTypedId<Guid>))]
    public sealed class HasUniqueDomainSignatureWithGuidIdAttribute : HasUniqueDomainSignatureAttributeBase
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return DoValidate(value, validationContext);
        }
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IEntityWithTypedId<long>))]
    public sealed class HasUniqueDomainSignatureWithBigIntIdAttribute : HasUniqueDomainSignatureAttributeBase
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return DoValidate(value, validationContext);
        }
    }
}
