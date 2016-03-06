namespace Tests.Northwind.Data
{
    using global::Northwind.Domain;
    using NHibernate;
    using NUnit.Framework;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class ProxyEqualityTests : DatabaseRepositoryTestsBase
    {
        private IRepository<Region> regionRepository ;

        private IRepositoryWithTypedId<Territory, string> territoryRepository ;

        /// <summary>
        /// Creates new <see cref="ISession"/>.
        /// </summary>
        public override void SetUp()
        {
            base.SetUp();
            this.regionRepository = new NHibernateRepository<Region>(TransactionManager, Session);
            this.territoryRepository = new NHibernateRepositoryWithTypedId<Territory, string>(TransactionManager, Session);
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