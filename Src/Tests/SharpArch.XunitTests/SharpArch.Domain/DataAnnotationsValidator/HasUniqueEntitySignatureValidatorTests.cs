namespace Tests.SharpArch.Domain.DataAnnotationsValidator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using FluentAssertions;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Domain.Validation;
    using Moq;
    using Xunit;


    public class HasUniqueObjectSignatureValidatorTests
    {
        readonly Mock<IServiceProvider> _serviceProviderMock;

        public HasUniqueObjectSignatureValidatorTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEntityDuplicateChecker)))
                .Returns(new DuplicateCheckerStub());
        }

        ValidationContext ValidationContextFor(object instance)
        {
            return new ValidationContext(instance, _serviceProviderMock.Object, null);
        }


        [HasUniqueDomainSignature]
        class Contractor : Entity<int>
        {
            [DomainSignature]
            public string Name { get; set; } = null!;
        }


        class DuplicateCheckerStub : IEntityDuplicateChecker
        {
            public bool DoesDuplicateExistWithTypedIdOf(IEntity entity)
                => entity switch
                {
                    Contractor contractor => !string.IsNullOrEmpty(contractor.Name) &&
                        string.Equals(contractor.Name, @"codai", StringComparison.OrdinalIgnoreCase),
                    User user => !string.IsNullOrEmpty(user.Ssn) && user.Ssn == "123-12-1234",
                    ObjectWithGuidId objectWithGuidId => !string.IsNullOrEmpty(objectWithGuidId.Name) &&
                        string.Equals(objectWithGuidId.Name, @"codai", StringComparison.OrdinalIgnoreCase),
                    // By default, simply return false for no duplicates found
                    _ => false
                };
        }


        [HasUniqueDomainSignature]
        class ObjectWithGuidId : Entity<Guid>
        {
            [DomainSignature]
            public string? Name { get; set; }
        }


        [HasUniqueDomainSignature]
        class User : Entity<string>
        {
            [DomainSignature]
            public string? Ssn { get; set; }
        }


        [Fact]
        public void CanVerifyThatDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor {Name = @"Codai"};
            ValidationContext validationContext = ValidationContextFor(contractor);
            IEnumerable<ValidationResult> invalidValues = contractor.ValidationResults(validationContext);
            foreach (ValidationResult invalidValue in invalidValues)
            {
                Debug.WriteLine(invalidValue.ErrorMessage);
            }

            contractor.IsValid(validationContext).Should().BeFalse();
        }

        [Fact]
        public void CanVerifyThatDuplicateExistsOfEntityWithGuidIdDuringValidationProcess()
        {
            var objectWithGuidId = new ObjectWithGuidId {Name = "codai"};

            objectWithGuidId.IsValid(ValidationContextFor(objectWithGuidId)).Should().BeFalse();

            objectWithGuidId = new ObjectWithGuidId {Name = "whatever"};
            objectWithGuidId.IsValid(ValidationContextFor(objectWithGuidId)).Should().BeTrue();
        }

        [Fact]
        public void CanVerifyThatDuplicateExistsOfEntityWithStringIdDuringValidationProcess()
        {
            var user = new User {Ssn = "123-12-1234"};
            user.IsValid(ValidationContextFor(user)).Should().BeFalse();
        }

        [Fact]
        public void CanVerifyThatNoDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor {Name = "Some unique name"};
            contractor.IsValid(ValidationContextFor(contractor)).Should().BeTrue();
        }

    }
}
