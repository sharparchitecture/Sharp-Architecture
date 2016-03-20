namespace Tests.Northwind.Data
{
    using global::Northwind.Domain;

    using NUnit.Framework;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class TerritoryRepositoryTests : DatabaseRepositoryTestsBase
    {
        private readonly IRepositoryWithTypedId<Territory, string> territoryRepository =
            new NHibernateRepositoryWithTypedId<Territory, string>();

        /// <summary>
        ///   WARNING: This is a very fragile test is will likely break over time.  It assumes 
        ///   a particular territory exists in the database and has exactly 1 employee.  Fragile 
        ///   tests that break over time can lead to people stopping to run tests at all.  In these
        ///   instances, it's very important to use a test DB with known data.
        /// </summary>
        [Test]
        public void CanLoadTerritory()
        {
            var territoryFromDb = this.territoryRepository.Get("48084");

            Assert.That(territoryFromDb.Description.Trim(), Is.EqualTo("Troy"));
            Assert.That(territoryFromDb.RegionBelongingTo.Description.Trim(), Is.EqualTo("Northern"));
            Assert.That(territoryFromDb.Employees.Count, Is.EqualTo(1));
        }
    }
}