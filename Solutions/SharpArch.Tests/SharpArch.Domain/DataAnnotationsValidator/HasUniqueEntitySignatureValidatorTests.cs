namespace Tests.SharpArch.Domain.DataAnnotationsValidator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using global::SharpArch.Domain;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate.NHibernateValidator;

    using Microsoft.Practices.ServiceLocation;

    using NUnit.Framework;

    [TestFixture]
    public class HasUniqueObjectSignatureValidatorTests
    {
        [Test]
        public void CanVerifyThatDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor { Name = "Codai" };
            IEnumerable<ValidationResult> invalidValues = contractor.ValidationResults();

            Assert.That(contractor.IsValid(), Is.False);

            foreach (var invalidValue in invalidValues)
            {
                Debug.WriteLine(invalidValue.ErrorMessage);
            }
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithGuidIdDuringValidationProcess()
        {
            var objectWithGuidId = new ObjectWithGuidId { Name = "codai" };
            Assert.That(objectWithGuidId.IsValid(), Is.False);

            objectWithGuidId = new ObjectWithGuidId { Name = "whatever" };
            Assert.That(objectWithGuidId.IsValid(), Is.True);
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithStringIdDuringValidationProcess()
        {
            var user = new User { SSN = "123-12-1234" };
            Assert.That(user.IsValid(), Is.False);
        }

        [Test]
        public void CanVerifyThatNoDuplicateExistsDuringValidationProcess()
        {
            var contractor = new Contractor { Name = "Some unique name" };
            Assert.That(contractor.IsValid());
        }

        public void InitServiceLocatorInitializer()
        {
            IWindsorContainer container = new WindsorContainer();

            container.Register(
                Component
                    .For(typeof(IEntityDuplicateChecker))
                    .ImplementedBy(typeof(DuplicateCheckerStub))
                    .Named("duplicateChecker"));

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }

        [Test]
        public void MayNotUseValidatorWithEntityHavingDifferentIdType()
        {
            var invalidCombination = new ObjectWithStringIdAndValidatorForIntId { Name = "whatever" };

            Assert.Throws<PreconditionException>(() => invalidCombination.ValidationResults());
        }

        [SetUp]
        public void SetUp()
        {
            this.InitServiceLocatorInitializer();
        }

        [HasUniqueDomainSignature]
        private class Contractor : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        private class DuplicateCheckerStub : IEntityDuplicateChecker
        {
            public bool DoesDuplicateExistWithTypedIdOf<IdT>(IEntityWithTypedId<IdT> entity)
            {
                Check.Require(entity != null);

                if (entity as Contractor != null)
                {
                    var contractor = entity as Contractor;
                    return !string.IsNullOrEmpty(contractor.Name) && contractor.Name.ToLower() == "codai";
                }
                else if (entity as User != null)
                {
                    var user = entity as User;
                    return !string.IsNullOrEmpty(user.SSN) && user.SSN.ToLower() == "123-12-1234";
                }
                else if (entity as ObjectWithGuidId != null)
                {
                    var objectWithGuidId = entity as ObjectWithGuidId;
                    return !string.IsNullOrEmpty(objectWithGuidId.Name) && objectWithGuidId.Name.ToLower() == "codai";
                }

                // By default, simply return false for no duplicates found
                return false;
            }
        }

        [HasUniqueDomainSignatureWithGuidId]
        private class ObjectWithGuidId : EntityWithTypedId<Guid>
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignature]
        private class ObjectWithStringIdAndValidatorForIntId : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignatureWithStringId]
        private class User : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string SSN { get; set; }
        }
    }
}