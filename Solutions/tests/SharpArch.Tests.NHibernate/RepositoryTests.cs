// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident

namespace Tests.SharpArch.NHibernate
{
    using FluentAssertions;
    using global::NHibernate;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    // ReSharper disable once TestFileNameWarning
    public class RepositoryTests
    {
        [Test]
        public void CanCastConcreteLinqRepositoryToInterfaceILinqRepository()
        {
            var session = new Mock<ISession>();
            var transactionManager = new Mock<IAsyncTransactionManager>();
            var concreteRepository = new LinqRepository<MyEntity>(transactionManager.Object, session.Object);

            concreteRepository.Should().BeAssignableTo<ILinqRepository<MyEntity>>();
        }
    }

    public class MyEntity
    {
        string Name { get; set; }
    }
}
