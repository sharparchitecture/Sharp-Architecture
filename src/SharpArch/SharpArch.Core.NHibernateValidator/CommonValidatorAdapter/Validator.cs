using System.Reflection;
using System.Collections.Generic;
using NHibernate.Validator.Engine;
using System;
using System.Collections.ObjectModel;
using SharpArch.Core.CommonValidator;
using SharpArch.Data.NHibernate;
using SharpArch.Core.NHibernateValidator.ValidatorProvider;

namespace SharpArch.Core.NHibernateValidator.CommonValidatorAdapter
{
    /// <summary>
    /// Provides an implementation of the <see cref="CommonValidator.IValidator" /> interface 
    /// which relies on NHibernate validator
    /// </summary>
    public class Validator : SharpArch.Core.CommonValidator.IValidator
    {
        public bool IsValid(object value) {
            Check.Require(value != null, "value to IsValid may not be null");

            return ValidatorEngine.IsValid(value);
        }

        public ICollection<IValidationResult> ValidationResultsFor(object value) {
            Check.Require(value != null, "value to ValidationResultsFor may not be null");

            InvalidValue[] invalidValues = ValidatorEngine.Validate(value);

            return ParseValidationResultsFrom(invalidValues);
        }

        private ICollection<IValidationResult> ParseValidationResultsFrom(InvalidValue[] invalidValues) {
            ICollection<IValidationResult> validationResults = new Collection<IValidationResult>();

            foreach (InvalidValue invalidValue in invalidValues) {
                validationResults.Add(new ValidationResult(invalidValue));
            }

            return validationResults;
        }

        private ValidatorEngine ValidatorEngine {
            get {
                return ValidatorEngineFactory.ValidatorEngine;
            }
        }
    }
}
