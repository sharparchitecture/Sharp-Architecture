﻿namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.Infrastructure.Caching;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;
    using Xunit;


    public class NHibernateSessionFactoryBuilderTests : IDisposable
    {
        private readonly string _tempFileName;

        /// <inheritdoc />
        public NHibernateSessionFactoryBuilderTests()
        {
            _tempFileName = "SharpArch.Tests." + Guid.NewGuid().ToString("D") + ".tmp";
        }

        /// <inheritdoc />
        public void Dispose()
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

        private static string GetConfigFullName()
        {
            const string defaultConfigFile = "sqlite-nhibernate-config.xml";
            return Path.Combine(DependencyList.GetAssemblyCodeBasePath(Assembly.GetExecutingAssembly()), defaultConfigFile);
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

        [Fact]
        public void CanInitializeWithConfigFileAndConfigurationFileCache()
        {
            Configuration configuration = new NHibernateSessionFactoryBuilder()
                .UseConfigurationCache(new NHibernateConfigurationFileCache("default", "NHibernateFactoryBuilderTests.bin"))
                .UseConfigFile(GetConfigFullName())
                .BuildConfiguration();

            configuration.Should().NotBeNull();

            configuration.BuildSessionFactory();
        }

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
                .WithFileDependency("SharpArch.NHibernate")
                .UsePersistenceConfigurer(persistenceConfigurer)
                .BuildConfiguration();

            configuration.BuildSessionFactory();
        }

        [Fact]
        public void DoesInitializeFailWhenCachingFileDependencyCannotBeFound()
        {
            Assert.Throws<FileNotFoundException>(
                () =>
                {
                    new NHibernateSessionFactoryBuilder()
                        // Random Guid value as dependency file to cause the exception
                        .UseConfigurationCache(new NHibernateConfigurationFileCache("default", _tempFileName))
                        .UseConfigFile(GetConfigFullName())
                        .WithFileDependency(Guid.NewGuid().ToString("D"))
                        .BuildConfiguration();
                });
        }

        [Fact]
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


    internal class InMemoryCache : NHibernateConfigurationCacheBase
    {
        private byte[] _data;
        private DateTime? _timestamp;

        /// <inheritdoc />
        public InMemoryCache([NotNull] string sessionName)
            : base(sessionName)
        {
        }

        /// <inheritdoc />
        protected override byte[] GetCachedConfiguration()
        {
            return _data;
        }

        /// <inheritdoc />
        protected override DateTime? GetCachedTimestampUtc()
        {
            return _timestamp;
        }

        /// <inheritdoc />
        protected override void SaveConfiguration(byte[] data, DateTime timestampUtc)
        {
            _data = data;
            _timestamp = timestampUtc;
        }
    }
}
