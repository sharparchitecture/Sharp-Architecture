namespace Tests.Northwind.Data
{
    using global::Northwind.Domain;

    using NUnit.Framework;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class ProxyEqualityTests : DatabaseRepositoryTestsBase
    {
        private IRepository<Region> regionRepository;

        private IRepositoryWithTypedId<Territory, string> territoryRepository;

        public override void SetUp()
        {
            base.SetUp();
            var transactionManager = new TransactionManager(Session);
            regionRepository = new NHibernateRepository<Region>(transactionManager, Session);
            territoryRepository = new NHibernateRepositoryWithTypedId<Territory, string>(transactionManager, Session);
        }

        [Test]
        public void ProxyEqualityTest()
        {
            var territory = this.territoryRepository.Get("31406");
            var proxiedRegion = territory.RegionBelongingTo;
            var unproxiedRegion = this.regionRepository.Get(4);

            Assert.IsTrue(proxiedRegion.Equals(unproxiedRegion));
            Assert.IsTrue(unproxiedRegion.Equals(proxiedRegion));
        }
    }
}