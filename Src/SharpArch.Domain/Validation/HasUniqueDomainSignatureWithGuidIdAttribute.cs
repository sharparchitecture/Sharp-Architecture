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
            return DoValidate<Guid>(value, validationContext);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HasUniqueDomainSignatureWithBigIntIdAttribute : HasUniqueDomainSignatureAttributeBase
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return DoValidate<long>(value, validationContext);
        }
    }
}
