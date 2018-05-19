// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.DelegateAllocation

// ReSharper disable HeapView.ObjectAllocation.Possible

namespace Tests.SharpArch.Domain.DataAnnotationsValidator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Domain.Validation;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class HasUniqueObjectSignatureValidatorTests
    {
        [SetUp]
        public void SetUp()
        {
            serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEntityDuplicateChecker)))
                .Returns(new DuplicateCheckerStub());
        }

        Mock<IServiceProvider> serviceProviderMock;

        ValidationContext ValidationContextFor(object instance)
        {
            return new ValidationContext(instance, serviceProviderMock.Object, null);
        }

        [HasUniqueDomainSignature]
        class Contractor : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        class DuplicateCheckerStub : IEntityDuplicateChecker
        {
            public bool DoesDuplicateExistWithTypedIdOf<IdT>(IEntityWithTypedId<IdT> entity)
            {
                Trace.Assert(entity != null);

                if (entity is Contractor) {
                    var contractor = entity as Contractor;
                    return !string.IsNullOrEmpty(contractor.Name) && contractor.Name.ToLower() == @"codai";
                }
                if (entity is User) {
                    var user = entity as User;
                    return !string.IsNullOrEmpty(user.SSN) && user.SSN.ToLower() == "123-12-1234";
                }
                if (entity is ObjectWithGuidId) {
                    var objectWithGuidId = entity as ObjectWithGuidId;
                    return !string.IsNullOrEmpty(objectWithGuidId.Name) && objectWithGuidId.Name.ToLower() == @"codai";
                }

                // By default, simply return false for no duplicates found
                return false;
            }
        }

        [HasUniqueDomainSignatureWithGuidId]
        class ObjectWithGuidId : EntityWithTypedId<Guid>
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignature]
        class ObjectWithStringIdAndValidatorForIntId : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignatureWithStringId]
        class User : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string SSN { get; set; }
        }

        [Test]
        public void CanVerifyThatDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor {Name = @"Codai"};
            ValidationContext validationContext = ValidationContextFor(contractor);
            IEnumerable<ValidationResult> invalidValues = contractor.ValidationResults(validationContext);

            Assert.That(contractor.IsValid(validationContext), Is.False);

            foreach (ValidationResult invalidValue in invalidValues) {
                Debug.WriteLine(invalidValue.ErrorMessage);
            }
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithGuidIdDuringValidationProcess()
        {
            var objectWithGuidId = new ObjectWithGuidId {Name = "codai"};

            Assert.That(objectWithGuidId.IsValid(ValidationContextFor(objectWithGuidId)), Is.False);

            objectWithGuidId = new ObjectWithGuidId {Name = "whatever"};
            Assert.That(objectWithGuidId.IsValid(ValidationContextFor(objectWithGuidId)), Is.True);
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithStringIdDuringValidationProcess()
        {
            var user = new User {SSN = "123-12-1234"};
            Assert.That(user.IsValid(ValidationContextFor(user)), Is.False);
        }

        [Test]
        public void CanVerifyThatNoDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor {Name = "Some unique name"};
            Assert.That(contractor.IsValid(ValidationContextFor(contractor)));
        }

        [Test]
        public void MayNotUseValidatorWithEntityHavingDifferentIdType()
        {
            var invalidCombination = new ObjectWithStringIdAndValidatorForIntId {Name = "whatever"};

            Assert.Throws<InvalidOperationException>(
                () => invalidCombination.ValidationResults(ValidationContextFor(invalidCombination)));
        }
    }
}
