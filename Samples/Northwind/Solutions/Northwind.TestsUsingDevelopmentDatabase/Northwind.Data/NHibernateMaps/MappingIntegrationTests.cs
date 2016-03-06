namespace Tests.Northwind.Data.NHibernateMaps
{
    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;

    using global::Northwind.Infrastructure.NHibernateMaps;

    using NUnit.Framework;

    using SharpArch.Data.NHibernate;
    using SharpArch.Testing.NHibernate;

    /// <summary>
    ///   Provides a means to verify that the target database is in compliance with all mappings.
    ///   Taken from http://ayende.com/Blog/archive/2006/08/09/NHibernateMappingCreatingSanityChecks.aspx.
    ///   If this is failing, the error will likely inform you that there is a missing table or column
    ///   which needs to be added to your database.
    /// </summary>
    [TestFixture]
    [Category("DB Tests")]
    public class MappingIntegrationTests
    {
        private Configuration configuration;

        [Test]
        public void CanConfirmDatabaseMatchesMappings()
        {
            var allClassMetadata = NHibernateSession.GetDefaultSessionFactory().GetAllClassMetadata();

            foreach (var entry in allClassMetadata)
            {
                NHibernateSession.Current.CreateCriteria(entry.Value.GetMappedClass(EntityMode.Poco)).SetMaxResults(0).
                    List();
            }
        }

        /// <summary>
        ///   Generates and outputs the database schema SQL to the console
        /// </summary>
        [Test]
        public void CanGenerateDatabaseSchema()
        {
            var session = NHibernateSession.GetDefaultSessionFactory().OpenSession();
            new SchemaExport(this.configuration).Execute(true, false, false, session.Connection, null);
        }

        [SetUp]
        public virtual void SetUp()
        {
            var mappingAssemblies = RepositoryTestsHelper.GetMappingAssemblies();
            this.configuration = NHibernateSession.Init(
                new SimpleSessionStorage(), 
                mappingAssemblies, 
                new AutoPersistenceModelGenerator().Generate(), 
                "../../../Northwind.Web/NHibernate.config");
        }

        [TearDown]
        public virtual void TearDown()
        {
            NHibernateSession.CloseAllSessions();
            NHibernateSession.Reset();
        }
    }
}