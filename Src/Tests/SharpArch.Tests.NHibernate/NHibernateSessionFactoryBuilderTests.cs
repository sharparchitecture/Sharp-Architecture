namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using NUnit.Framework;


    [TestFixture]
    class NHibernateSessionFactoryBuilderTests
    {
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
        [Ignore("ConfigurationFileCache needs to be implemented on .Net Standard")]
        public void CanInitializeWithConfigFileAndConfigurationFileCache()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigurationCache(new NHibernateConfigurationFileCache(new[] {"SharpArch.NHibernate"}))
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
        public void DoesInitializeFailWhenCachingFileDependencyCannotBeFound()
        {
            Assert.Throws<FileNotFoundException>(
                () => {
                    new NHibernateSessionFactoryBuilder()
                        // Random Guid value as dependency file to cause the exception
                        .UseConfigurationCache(new NHibernateConfigurationFileCache(new[] {Guid.NewGuid().ToString()}))
                        .UseConfigFile(GetConfigFullName())
                        .BuildConfiguration();
                });
        }

        [Test]
        public void ShouldPersistExposedConfigurationChanges()
        {
            var cache = new InMemoryCache();

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


    class InMemoryCache : INHibernateConfigurationCache
    {
        MemoryStream _memoryStream;

        public InMemoryCache()
        {
            _memoryStream = new MemoryStream();
        }

        public Configuration LoadConfiguration(
            string configKey, string configPath,
            IEnumerable<string> mappingAssemblies)
        {
            if (_memoryStream.Length == 0)
                return null;

            _memoryStream.Position = 0;
            return ConfigurationFileCache.Load(_memoryStream);
        }

        public void SaveConfiguration(string configKey, Configuration config)
        {
            _memoryStream.SetLength(0);
            ConfigurationFileCache.Save(_memoryStream, config);
        }
    }
}
