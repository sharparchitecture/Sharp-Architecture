namespace Tests.Northwind.Data.Organization
{
    using global::Northwind.Domain.Organization;

    using NUnit.Framework;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class EmployeeRepositoryTests : DatabaseRepositoryTestsBase
    {
        private readonly IRepository<Employee> employeeRepository = new NHibernateRepository<Employee>();

        /// <summary>
        ///   WARNING: This is a very fragile test is will likely break over time.  It assumes 
        ///   a particular employee exists in the database and has exactly 7 territories.  Fragile 
        ///   tests that break over time can lead to people stopping to run tests at all.  In these
        ///   instances, it's very important to use a test DB with known data.
        /// </summary>
        [Test]
        public void CanLoadEmployee()
        {
            var employeeFromDb = this.employeeRepository.Get(2);

            Assert.That(employeeFromDb.FirstName, Is.EqualTo("Andrew"));
            Assert.That(employeeFromDb.LastName, Is.EqualTo("Fuller"));
            Assert.That(employeeFromDb.Territories.Count, Is.EqualTo(7));
        }
    }
}