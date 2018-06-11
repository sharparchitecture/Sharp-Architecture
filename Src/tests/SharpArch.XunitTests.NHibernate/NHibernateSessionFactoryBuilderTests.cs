// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation

// ReSharper disable HeapView.ObjectAllocation

// ReSharper disable MissingXmlDoc
namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.Domain;
    using global::SharpArch.NHibernate;
    using Xunit;


    public class NHibernateSessionFactoryBuilderTests
    {
        static string GetConfigFullName()
        {
            const string defaultConfigFile = "sqlite-nhibernate-config.xml";
            return Path.Combine(NHibernateConfigurationFileCache.GetAssemblyCodeBasePath(Assembly.GetExecutingAssembly()), defaultConfigFile);
        }

        [Fact]
        public void CanExposeConfiguration()
        {
            var exposeCalled = false;
            Action<Configuration> configure = c => { exposeCalled = true; };

            new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .ExposeConfiguration(configure)
                .BuildConfiguration();

            exposeCalled.Should().BeTrue();
        }

        [Fact]
        public void CanInitializeWithConfigFile()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            configuration.Should().NotBeNull();

            configuration.BuildSessionFactory();
        }

#if NETFULL
        [Fact]
        public void CanInitializeWithConfigFileAndConfigurationFileCache()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigurationCache(new NHibernateConfigurationFileCache(new[] {"SharpArch.NHibernate"}))
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            configuration.Should().NotBeNull();

            configuration.BuildSessionFactory();
        }

        [Fact]
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
#endif

        [Fact]
        public void CanInitializeWithPersistenceConfigurerAndConfigFile()
        {
            SQLiteConfiguration persistenceConfigurer =
                SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UsePersistenceConfigurer(persistenceConfigurer)
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            configuration.BuildSessionFactory();
        }

        [Fact]
        public void CanInitializeWithPersistenceConfigurerAndNoConfigFile()
        {
            SQLiteConfiguration persistenceConfigurer =
                SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));

            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UsePersistenceConfigurer(persistenceConfigurer)
                .BuildConfiguration();

            configuration.BuildSessionFactory();
        }

        [Fact]
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


        [Fact]
        public void WhenUsingDataAnnotationValidators_ShouldKeepRegisteredPreInsertEventListeners()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigFile(GetConfigFullName())
                .UseDataAnnotationValidators(true)
                .BuildConfiguration();

            configuration.EventListeners.PreInsertEventListeners.Should().Contain(l => l is PreInsertListener);
        }

        [Fact]
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
        MemoryStream memoryStream;

        public InMemoryCache()
        {
            memoryStream = new MemoryStream();
        }

        public Configuration LoadConfiguration(
            string configKey, string configPath,
            IEnumerable<string> mappingAssemblies)
        {
            if (memoryStream.Length == 0)
                return null;

            memoryStream.Position = 0;
            return FileCache.Load<Configuration>(memoryStream);
        }

        public void SaveConfiguration(string configKey, Configuration config)
        {
            memoryStream.SetLength(0);
            FileCache.Save(memoryStream, config);
        }
    }
}
