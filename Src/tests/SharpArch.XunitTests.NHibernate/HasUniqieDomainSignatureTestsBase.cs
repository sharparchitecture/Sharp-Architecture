namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Domain;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using global::SharpArch.Testing.NHibernate;
    using global::SharpArch.Testing.Xunit.NHibernate;
    using Mappings;
    using Moq;


    public class DomainSignatureDbSetup : TestDatabaseSetup
    {
        public DomainSignatureDbSetup()
            : base(Assembly.GetExecutingAssembly().CodeBase,
                new[]
                {
                    typeof(ObjectWithGuidId).Assembly,
                    typeof(TestsPersistenceModelGenerator).Assembly
                })
        {
        }
    }


    public abstract class HasUniqueDomainSignatureTestsBase : TransientDatabaseTests<DomainSignatureDbSetup>
    {
        protected Mock<IServiceProvider> ServiceProviderMock;
        protected ValidationContext ValidationContext;

        public HasUniqueDomainSignatureTestsBase()
            : base(new DomainSignatureDbSetup())
        {
            ServiceProviderMock = new Mock<IServiceProvider>();
            ServiceProviderMock.Setup(sp => sp.GetService(typeof(IEntityDuplicateChecker)))
                .Returns(new EntityDuplicateChecker(Session));
        }

        /// <summary>
        ///     Create validation context for given object.
        /// </summary>
        /// <param name="objectToValidate"></param>
        /// <returns></returns>
        protected ValidationContext ValidationContextFor(object objectToValidate)
        {
            return new ValidationContext(objectToValidate, ServiceProviderMock.Object, null);
        }
    }
}
