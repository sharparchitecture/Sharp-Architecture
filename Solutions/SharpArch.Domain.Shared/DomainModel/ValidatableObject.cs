namespace SharpArch.Domain.DomainModel
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using JetBrains.Annotations;

    /// <summary>
    ///     Serves as the base class for objects that are validatable.
    /// </summary>
    [Serializable]
    [PublicAPI]
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global", Justification = "Public API")]
    public abstract class ValidatableObject : BaseObject
    {
        /// <summary>
        ///     Determines whether this instance is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid(ValidationContext validationContext)
        {
            return this.ValidationResults(validationContext).Count == 0;
        }

        /// <summary>
        ///     Validates all properties of the object and returns the validation results if any of
        ///     them were deemed invalid.
        /// </summary>
        /// <returns>A collection of validation results.</returns>
        public virtual ICollection<ValidationResult> ValidationResults(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(this, validationContext, validationResults, true);
            return validationResults;
        }
    }
}