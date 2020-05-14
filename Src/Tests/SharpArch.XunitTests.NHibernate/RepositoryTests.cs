namespace Tests.SharpArch.NHibernate
{
    using FluentAssertions;
    using global::NHibernate;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using global::SharpArch.NHibernate.Impl;
    using global::SharpArch.NHibernate.MultiDb;
    using Moq;
    using Xunit;


    public class RepositoryTests
    {
        [Fact]
        public void CanCastConcreteLinqRepositoryToInterfaceILinqRepository()
        {
            var session = new Mock<ISession>();
            var transactionManager = new Mock<INHibernateTransactionManager>();
            transactionManager.SetupGet(t => t.Session).Returns(session.Object);

            var sessionRegistry = new Mock<INHibernateSessionRegistry>();
            sessionRegistry.Setup(x => x.GetNHibernateTransactionManager(DatabaseIdentifier.Default)).Returns(transactionManager.Object);

            var concreteRepository = new LinqRepository<MyEntity, int>(sessionRegistry.Object, DefaultDatabaseIdentifierProvider.Instance);

            concreteRepository.Should().BeAssignableTo<ILinqRepository<MyEntity, int>>();
        }
    }


    public class MyEntity : Entity<int>
    {
        string Name { get; set; }
    }
}
