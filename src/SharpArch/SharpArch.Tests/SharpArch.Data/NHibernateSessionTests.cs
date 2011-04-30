using FluentNHibernate.Cfg.Db;
using NUnit.Framework;
using SharpArch.Data.NHibernate;

namespace Tests.SharpArch.Data
{
    [Ignore]
    [TestFixture]
    public class NHibernateSessionTests
    {
        [SetUp]
        public void SetUp() {
            NHibernateSession.Reset();
        }

        [Test]
        public void CanInitializeWithConfigFile() {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(),
                mappingAssemblies, configFile);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndConfigFile() {
            var configFile = "sqlite-nhibernate-config.xml";
            var persistenceConfigurer = SQLiteConfiguration.Standard
                .ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"))
                .ProxyFactoryFactory("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");

            var mappingAssemblies = new string[] { };

            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(),
                mappingAssemblies, null, configFile, null, null, persistenceConfigurer, null);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithPersistenceConfigurerAndNoConfigFile() {
            var persistenceConfigurer = SQLiteConfiguration.Standard
                .ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"))
                .ProxyFactoryFactory("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");

            var mappingAssemblies = new string[] { };

            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(),
                mappingAssemblies, null, null, null, null, persistenceConfigurer, null);

            Assert.That(configuration, Is.Not.Null);

        }

        [Test]
        public void CanInitializeWithConfigFileAndConfigurationFileCache() {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                new string[] { "SharpArch.Core" });
            var configuration = NHibernateSession.Init(
                new SimpleSessionStorage(),
                mappingAssemblies, configFile);

            Assert.That(configuration, Is.Not.Null);
        }

        [Test]
        public void CanInitializeWithConfigFileAndConfigurationFileCacheAndSecondDatabaseConfiguration() {
            var configFile = "sqlite-nhibernate-config.xml";
            var mappingAssemblies = new string[] { };
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                new string[] { "SharpArch.Core" });
            var configuration1 = NHibernateSession.Init(
                new SimpleSessionStorage(),
                mappingAssemblies, configFile);
            var configuration2 = NHibernateSession.AddConfiguration(
                "secondDatabase",
                new string[] { },
                null,
                configFile,
                null, null, null, null);

            Assert.That(configuration1, Is.Not.Null);
            Assert.That(configuration2, Is.Not.Null);
        }

        [Test]
        public void DoesInitializeFailWhenCachingFileDependencyCannotBeFound() {
            Assert.Throws<System.IO.FileNotFoundException>(() =>
            {
                var configFile = "sqlite-nhibernate-config.xml";
                var mappingAssemblies = new string[] { };
                // Random Guid value as dependency file to cause the exception
                NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                    new string[] { System.Guid.NewGuid().ToString() });
                var configuration = NHibernateSession.Init(
                    new SimpleSessionStorage(),
                    mappingAssemblies, configFile);
            });
        }

        [Test]
        public void DoesChangingTheConfigurationCacheAfterInitCauseException() {
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                var configFile = "sqlite-nhibernate-config.xml";
                var mappingAssemblies = new string[] { };
                NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                    new string[] { "SharpArch.Core" });
                var configuration = NHibernateSession.Init(
                    new SimpleSessionStorage(),
                    mappingAssemblies, configFile);
                // Set ConfigurationCache to a different file cache object
                NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(
                    new string[] { "SharpArch.Core" });
            });

        }
    }
}