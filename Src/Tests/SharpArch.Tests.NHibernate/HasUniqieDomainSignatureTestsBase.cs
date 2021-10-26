namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using global::SharpArch.Testing.NHibernate;
    using global::SharpArch.Testing.NUnit.NHibernate;
    using Mappings;
    using Moq;
    using NUnit.Framework;


    class HasUniqueDomainSignatureTestsBase : RepositoryTestsBase
    {
        protected Mock<IServiceProvider> ServiceProviderMock = null!;

        public HasUniqueDomainSignatureTestsBase()
            : base(new TestDatabaseSetup(
                TestContext.CurrentContext.TestDirectory,
                typeof(TestsPersistenceModelGenerator),
                new[] {typeof(TestsPersistenceModelGenerator).Assembly}
            ))
        {
        }

        protected override void LoadTestData()
        {
        }

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            ServiceProviderMock = new Mock<IServiceProvider>();
            ServiceProviderMock.Setup(sp => sp.GetService(typeof(IEntityDuplicateChecker)))
                .Returns(new EntityDuplicateChecker(Session));
        }

        protected ValidationContext ValidationContextFor(object objectToValidate)
        {
            return new ValidationContext(objectToValidate, ServiceProviderMock.Object, null);
        }
    }
}
