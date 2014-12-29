namespace SharpArch.Features.Tests.SharpArch.Features.NHibernate
{
    using System;
    using System.IO;

    using FluentNHibernate.Cfg.Db;

    using global::SharpArch.NHibernate;

    using NUnit.Framework;

    [TestFixture]
    public class NHibernateSessionTests
    {
        [Test]
        public void CanInitializeWithConfigFile()
        {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            var configuration = NHibernateSession.Init(new SimpleSessionStorage(), mappingAssemblies, configFile);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithConfigFileAndConfigurationFileCache()
        {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(new[] { "SharpArch.NHibernate" });
            var configuration = NHibernateSession.Init(new SimpleSessionStorage(), mappingAssemblies, configFile);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithConfigFileAndConfigurationFileCacheAndSecondDatabaseConfiguration()
        {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(new[] { "SharpArch.NHibernate" });
            var configuration1 = NHibernateSession.Init(new SimpleSessionStorage(), mappingAssemblies, configFile);
            var configuration2 = NHibernateSession.AddConfiguration(
                "secondDatabase", new string[] { }, null, configFile, null, null, null);

            Assert.That(configuration1, Is.Not.Null);
            Assert.That(configuration2, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndConfigFile()
        {
            var configFile = "sqlite-nhibernate-config.xml";
            var persistenceConfigurer = SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            var mappingAssemblies = new string[] { };

            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(), mappingAssemblies, null, configFile, null, null, persistenceConfigurer);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndNoConfigFile()
        {
            var persistenceConfigurer = SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            var mappingAssemblies = new string[] { };

            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(), mappingAssemblies, null, null, null, null, persistenceConfigurer);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void DoesChangingTheConfigurationCacheAfterInitCauseException()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    var configFile = "sqlite-nhibernate-config.xml";
                    var mappingAssemblies = new string[] { };
                    NHibernateSession.ConfigurationCache =
                        new NHibernateConfigurationFileCache(new[] { "SharpArch.NHibernate" });
                    var configuration = NHibernateSession.Init(
                        new SimpleSessionStorage(), mappingAssemblies, configFile);

                    // Set ConfigurationCache to a different file cache object
                    NHibernateSession.ConfigurationCache =
                        new NHibernateConfigurationFileCache(new[] { "SharpArch.NHibernate" });
                });
        }

        [Test]
        public void DoesInitializeFailWhenCachingFileDependencyCannotBeFound()
        {
            Assert.Throws<FileNotFoundException>(
                () =>
                {
                    var configFile = "sqlite-nhibernate-config.xml";
                    var mappingAssemblies = new string[] { };

                    // Random Guid value as dependency file to cause the exception
                    NHibernateSession.ConfigurationCache =
                        new NHibernateConfigurationFileCache(new[] { Guid.NewGuid().ToString() });
                    var configuration = NHibernateSession.Init(
                        new SimpleSessionStorage(), mappingAssemblies, configFile);
                });
        }

        [SetUp]
        public void SetUp()
        {
            NHibernateSession.Reset();
        }
    }
}