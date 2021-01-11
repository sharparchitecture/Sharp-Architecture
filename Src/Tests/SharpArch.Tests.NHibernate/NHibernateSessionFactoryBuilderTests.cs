namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.IO;
    using FluentAssertions;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using NUnit.Framework;


    [TestFixture]
    class NHibernateSessionFactoryBuilderTests
    {
        [SetUp]
        public void SetUp()
        {
            _tempFileName = "SharpArch.Tests." + Guid.NewGuid().ToString("D") + ".tmp";
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (File.Exists(_tempFileName)) File.Delete(_tempFileName);
            }
            // ReSharper disable once CatchAllClause
            catch
            {
                // ignored
            }
        }

        string _tempFileName;

        static string GetConfigFullName()
        {
            const string defaultConfigFile = "sqlite-nhibernate-config.xml";
            return Path.Combine(TestContext.CurrentContext.TestDirectory, defaultConfigFile);
        }

        [Test]
        public void CanExposeConfiguration()
        {
            var exposeCalled = false;

            void configure(Configuration c)
            {
                exposeCalled = true;
            }

            new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .ExposeConfiguration(configure)
                .BuildConfiguration();

            exposeCalled.Should().BeTrue();
        }

        [Test]
        public void CanInitializeWithConfigFile()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            Assert.That(configuration, Is.Not.Null);

            configuration.BuildSessionFactory();
        }

        [Test]
        public void CanInitializeWithConfigFileAndConfigurationFileCache()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            Assert.That(configuration, Is.Not.Null);

            configuration.BuildSessionFactory();
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndConfigFile()
        {
            SQLiteConfiguration persistenceConfigurer =
                SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UsePersistenceConfigurer(persistenceConfigurer)
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            Assert.That(configuration, Is.Not.Null);
            configuration.BuildSessionFactory();
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndNoConfigFile()
        {
            SQLiteConfiguration persistenceConfigurer =
                SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UsePersistenceConfigurer(persistenceConfigurer)
                .BuildConfiguration();

            Assert.That(configuration, Is.Not.Null);
            configuration.BuildSessionFactory();
        }

        [Test]
        public void WhenUsingDataAnnotationValidators_ShouldKeepRegisteredPreInsertEventListeners()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .UseDataAnnotationValidators(true)
                .BuildConfiguration();

            configuration.EventListeners.PreInsertEventListeners.Should().Contain(l => l is PreInsertListener);
        }

        [Test]
        public void WhenUsingDataAnnotationValidators_ShouldKeepRegisteredPreUpdateEventListeners()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .UseDataAnnotationValidators(true)
                .BuildConfiguration();

            configuration.EventListeners.PreUpdateEventListeners.Should().Contain(l => l is PreUpdateListener);
        }
    }
}
