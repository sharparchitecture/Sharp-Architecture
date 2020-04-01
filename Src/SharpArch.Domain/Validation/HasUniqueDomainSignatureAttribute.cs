namespace SharpArch.Domain.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using JetBrains.Annotations;
    using SharpArch.Domain.DomainModel;

    /// <summary>
    ///     Provides a class level validator for determining if the entity has a unique domain signature
    ///     when compared with other entries in the database.
    ///     Due to the fact that .NET does not support generic attributes, this only works for entity
    ///     types having an Id of type int.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    [BaseTypeRequired(typeof(IEntity))]
    public sealed class HasUniqueDomainSignatureAttribute : HasUniqueDomainSignatureAttributeBase
    {
        /// <summary>
        ///     Returns true if entity's domain signature is unique in database.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return DoValidate(value, validationContext);
        }
    }
}
