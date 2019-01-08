namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;
    using NUnit.Framework;


    [TestFixture]
    class NHibernateSessionFactoryBuilderTests
    {
        private string _tempFileName;

        static string GetConfigFullName()
        {
            const string defaultConfigFile = "sqlite-nhibernate-config.xml";
            return Path.Combine(TestContext.CurrentContext.TestDirectory, defaultConfigFile);
        }

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
                .UseConfigurationCache(new NHibernateConfigurationFileCache("default", _tempFileName))
                .WithFileDependency("SharpArch.NHibernate")
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
                .WithFileDependency("SharpArch.NHibernate")
                .UsePersistenceConfigurer(persistenceConfigurer)
                .BuildConfiguration();

            Assert.That(configuration, Is.Not.Null);
            configuration.BuildSessionFactory();
        }

        [Test]
        public void DoesInitializeFailWhenCachingFileDependencyCannotBeFound()
        {
            Assert.Throws<FileNotFoundException>(
                () => {
                    new NHibernateSessionFactoryBuilder()
                        // Random Guid value as dependency file to cause the exception
                        .UseConfigurationCache(new NHibernateConfigurationFileCache("default", _tempFileName))
                        .WithFileDependency(Guid.NewGuid().ToString("D"))
                        .UseConfigFile(GetConfigFullName())
                        .BuildConfiguration();
                });
        }

        [Test]
        public void ShouldPersistExposedConfigurationChanges()
        {
            var cache = new InMemoryCache("default");

            new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .ExposeConfiguration(c => c.SetProperty("connection.connection_string", "updated-connection"))
                .UseConfigurationCache(cache)
                .BuildConfiguration();

            Configuration config = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .UseConfigurationCache(cache)
                .BuildConfiguration();

            config.Properties["connection.connection_string"].Should().Be("updated-connection");
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


    class InMemoryCache : NHibernateConfigurationCacheBase
    {
        private DateTime? _timestamp;
        private byte[] _data;

        /// <inheritdoc />
        public InMemoryCache([NotNull] string sessionName)
            : base(sessionName)
        {
        }

        /// <inheritdoc />
        protected override byte[] GetCachedConfiguration() => _data;

        /// <inheritdoc />
        protected override DateTime? GetCachedTimestampUtc() => _timestamp;

        /// <inheritdoc />
        protected override void SaveConfiguration(byte[] data, DateTime timestampUtc)
        {
            _data = data;
            _timestamp = timestampUtc;
        }
    }
}
