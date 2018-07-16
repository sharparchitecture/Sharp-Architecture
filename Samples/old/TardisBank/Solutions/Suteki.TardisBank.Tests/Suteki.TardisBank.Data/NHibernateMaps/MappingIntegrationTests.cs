namespace Suteki.TardisBank.Tests.Suteki.TardisBank.Data.NHibernateMaps
{
    using System;
    using System.IO;

    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;

    using NUnit.Framework;

    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    using global::Suteki.TardisBank.Infrastructure.NHibernateMaps;

    /// <summary>
    /// Provides a means to verify that the target database is in compliance with all mappings.
    /// Taken from http://ayende.com/Blog/archive/2006/08/09/NHibernateMappingCreatingSanityChecks.aspx.
    /// 
    /// If this is failing, the error will likely inform you that there is a missing table or column
    /// which needs to be added to your database.
    /// </summary>
    [TestFixture]
    [Category("DatabaseTests")]
    [Category("IntegrationTests")]
    class MappingIntegrationTests
    {
        private Configuration configuration;
        private ISessionFactory sessionFactory;
        private ISession session;

        [SetUp]
        public virtual void SetUp()
        {
            string[] mappingAssemblies = TestDatabaseInitializer.GetMappingAssemblies(TestContext.CurrentContext.TestDirectory);
            var nhibernateConfigPath = new Uri(CalculatePath("../../../../Solutions/Suteki.TardisBank.Web.Mvc/NHibernate.config")).LocalPath;
            this.configuration = new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(mappingAssemblies)
                .UseAutoPersistenceModel(new AutoPersistenceModelGenerator().Generate())
                .UseConfigFile(nhibernateConfigPath)
                .BuildConfiguration();
            sessionFactory = configuration.BuildSessionFactory();
            session = sessionFactory.OpenSession();
        }

        [TearDown]
        public virtual void TearDown()
        {
            sessionFactory?.Dispose();
        }

        [Test]
        public void CanConfirmDatabaseMatchesMappings()
        {
            var allClassMetadata = sessionFactory.GetAllClassMetadata();

            foreach (var entry in allClassMetadata)
            {
                session.CreateCriteria(entry.Value.GetMappedClass(EntityMode.Poco))
                     .SetMaxResults(0).List();
            }
        }

        /// <summary>
        /// Generates and outputs the database schema SQL to the console
        /// </summary>
        [Test]
        public void CanGenerateDatabaseSchema()
        {
            using (TextWriter stringWriter = new StreamWriter(CalculatePath("../../../../Database/UnitTestGeneratedSchema.sql")))
            {
                new SchemaExport(this.configuration).Execute(true, false, false, session.Connection, stringWriter);
            }
        }

        /// <summary>
        /// Creates/Updates database schema, this runs on database configured in 
        /// Mvc project and is marked as Explicit because it changes the database.
        /// </summary>
        [Test]
        [Explicit]
        public void CanCreateDatabase()
        {
            new SchemaExport(this.configuration).Execute(false, true, false);
        }

        /// <summary>
        /// Calculates path based on test assembly folder
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string CalculatePath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }
    }
}
