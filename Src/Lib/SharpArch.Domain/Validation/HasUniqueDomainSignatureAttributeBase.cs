namespace SharpArch.Domain.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using JetBrains.Annotations;
    using DomainModel;
    using PersistenceSupport;

    /// <summary>
    ///     Performs validation of domain signature uniqueness.
    /// </summary>
    /// <remarks>
    ///     Performs database search to ensure no other entity with same domain signature exists.
    /// </remarks>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    /// <seealso cref="DomainSignatureAttribute" />
    /// .
    [PublicAPI]
    [BaseTypeRequired(typeof(IEntity))]
    public class HasUniqueDomainSignatureAttributeBase : ValidationAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HasUniqueDomainSignatureAttributeBase" /> class.
        /// </summary>
        protected HasUniqueDomainSignatureAttributeBase()
            : base("Provided values matched an existing, duplicate entity")
        { }

        /// <summary>
        ///     Gets a value that indicates whether the attribute requires validation context.
        /// </summary>
        public override bool RequiresValidationContext => true;

        /// <summary>
        ///     Performs database lookup to ensure domain signature uniqueness.
        /// </summary>
        /// <param name="value">The entity.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        ///     <see cref="IEntityDuplicateChecker" />can not be resolved from
        ///     <paramref name="validationContext" />.
        /// </exception>
        protected ValidationResult? DoValidate(object? value, ValidationContext validationContext)
        {
            var entityToValidate = value as IEntity;
            if (entityToValidate == null)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "This validator must be used at the class level of an " + nameof(IEntity) + ". The type you provided was '{0}'.",
                    (value?.GetType() as object) ?? "null"));

            var duplicateChecker =
                (IEntityDuplicateChecker?) validationContext.GetService(typeof(IEntityDuplicateChecker));
            if (duplicateChecker == null)
                throw new InvalidOperationException(
                    $"'{nameof(IEntityDuplicateChecker)}' can not be resolved from validation context.");
            return duplicateChecker.DoesDuplicateExistWithTypedIdOf(entityToValidate)
                ? new ValidationResult(ErrorMessage)
                : ValidationResult.Success;
        }
    }
}
