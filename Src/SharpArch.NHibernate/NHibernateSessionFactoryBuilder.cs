﻿namespace SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using global::FluentNHibernate.Automapping;
    using global::FluentNHibernate.Cfg;
    using global::FluentNHibernate.Cfg.Db;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using JetBrains.Annotations;
    using NHibernateValidator;

    /// <summary>
    ///     Creates NHibernate SessionFactory <see cref="ISessionFactory" />
    /// </summary>
    /// <remarks>
    ///     Transient object, session factory must be registered as singleton in DI Container.
    /// </remarks>
    [PublicAPI]
    public class NHibernateSessionFactoryBuilder
    {
        /// <summary>
        /// Default configuration file name.
        /// </summary>
        public const string DefaultNHibernateConfigFileName = @"Hibernate.cfg.xml";

        /// <summary>
        /// Default NHibernate session factory key.
        /// </summary>
        public static readonly string DefaultConfigurationName = "nhibernate.current_session";

        INHibernateConfigurationCache _configurationCache;

        AutoPersistenceModel _autoPersistenceModel;
        string _configFile;
        readonly List<Assembly> _mappingAssemblies;
        IPersistenceConfigurer _persistenceConfigurer;
        IDictionary<string, string> _properties;
        bool _useDataAnnotationValidators;
        Action<Configuration> _exposeConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateSessionFactoryBuilder"/> class.
        /// </summary>
        public NHibernateSessionFactoryBuilder()
        {
            _configurationCache = NullNHibernateConfigurationCache.Null;
            _mappingAssemblies = new List<Assembly>();
        }


        /// <summary>
        /// Creates the session factory.
        /// </summary>
        /// <returns> NHibernate session factory <see cref="ISessionFactory"/>.</returns>
        [NotNull]
        public ISessionFactory BuildSessionFactory()
        {
            var configuration = BuildConfiguration();
            return configuration.BuildSessionFactory();
        }


        /// <summary>
        /// Builds NHibernate configuration.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Any changes made to configuration object <b>will not be persisted</b> in configuration cache.
        /// This can be useful to make dynamic changes to configuration or in case changes cannot be serialized
        /// (e.g. event listeners are not marked with <see cref="SerializableAttribute"/>.
        /// </para>
        /// <para>
        /// To make persistent changes use <seealso cref="ExposeConfiguration"/>.
        /// </para>
        /// </remarks>
        [NotNull]
        public Configuration BuildConfiguration()
        {
            var configuration = _configurationCache.LoadConfiguration(DefaultConfigurationName, _configFile, _mappingAssemblies.Select(x=>x.Location));

            if (configuration == null)
            {
                configuration = LoadExternalConfiguration();
                configuration = ApplyCustomSettings(configuration);
                _configurationCache.SaveConfiguration(DefaultConfigurationName, configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Allows to alter configuration before creating NHibernate configuration.
        /// </summary>
        /// <remarks>
        /// Changes to configuration will be persisted in configuration cache, if it is enabled.
        /// In case changes must not be persisted in cache, they must be applied after <seealso cref="BuildConfiguration"/>.
        /// </remarks>
        [NotNull]
        public NHibernateSessionFactoryBuilder ExposeConfiguration([NotNull] Action<Configuration> config)
        {
            _exposeConfiguration = config ?? throw new ArgumentNullException(nameof(config));
            return this;
        }

        bool ShouldExposeConfiguration()
        {
            return _exposeConfiguration != null;
        }

        /// <summary>
        /// Allows to cache compiled NHibernate configuration.
        /// </summary>
        /// <param name="configurationCache">The configuration cache, see <see cref="INHibernateConfigurationCache"/>. </param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Please provide configuration cache instance.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseConfigurationCache(
            [NotNull] INHibernateConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache ?? throw new ArgumentNullException(nameof(configurationCache), "Please provide configuration cache instance.");
            return this;
        }

        /// <summary>
        /// Allows to specify additional assemblies containing FluentNHibernate mappings.
        /// </summary>
        /// <param name="mappingAssemblies">The mapping assemblies.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Mapping assemblies are not specified.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder AddMappingAssemblies([NotNull] IEnumerable<Assembly> mappingAssemblies)
        {
            if (mappingAssemblies == null) throw new ArgumentNullException(nameof(mappingAssemblies), "Mapping assemblies are not specified.");

            _mappingAssemblies.AddRange(mappingAssemblies);
            return this;
        }

        /// <summary>
        /// Allows to specify FluentNhibernate auto-persistence model to use..
        /// </summary>
        /// <param name="autoPersistenceModel">The automatic persistence model.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseAutoPersistenceModel(
            [NotNull] AutoPersistenceModel autoPersistenceModel)
        {
            _autoPersistenceModel = autoPersistenceModel ?? throw new ArgumentNullException(nameof(autoPersistenceModel));
            return this;
        }

        /// <summary>
        /// Allows to specify additional NHibernate properties, see http://nhibernate.info/doc/nhibernate-reference/session-configuration.html.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="properties"/> is <c>null</c>.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseProperties([NotNull] IDictionary<string, string> properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            return this;
        }


        /// <summary>
        /// Allows to use Data Annotations and <see cref="Validator"/> to validate entities before insert/update.
        /// </summary>
        /// <remarks>
        /// See https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations%28v=vs.110%29.aspx for details about Data Annotations.
        /// </remarks>
        /// <seealso cref="DataAnnotationsEventListener"/>.
        [NotNull]
        public NHibernateSessionFactoryBuilder UseDataAnnotationValidators(bool addDataAnnotatonValidators)
        {
            _useDataAnnotationValidators = addDataAnnotatonValidators;
            return this;
        }


        /// <summary>
        /// Allows to specify nhibernate configuration file.
        /// </summary>
        /// <remarks>
        /// See http://nhibernate.info/doc/nhibernate-reference/session-configuration.html#configuration-xmlconfig for more details
        /// </remarks>
        /// <exception cref="System.ArgumentException">NHibernate config was not specified.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseConfigFile(string nhibernateConfigFile)
        {
            if (string.IsNullOrWhiteSpace(nhibernateConfigFile))
                throw new ArgumentException("NHibernate config was not specified.", nameof(nhibernateConfigFile));
            
            _configFile = nhibernateConfigFile;

            return this;
        }

        /// <summary>
        /// Allows to specify custom configuration using <see cref="IPersistenceConfigurer"/>.
        /// </summary>
        /// <param name="persistenceConfigurer">The persistence configurer.</param>
        /// <example>
        /// <code>
        /// var persistenceConfigurer =
        ///   SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));
        /// var configuration = new NHibernateSessionFactoryBuilder()
        ///   .UsePersistenceConfigurer(persistenceConfigurer);
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException"><paramref name="persistenceConfigurer"/> is <c>null</c>.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UsePersistenceConfigurer(
            [NotNull] IPersistenceConfigurer persistenceConfigurer)
        {
            _persistenceConfigurer = persistenceConfigurer ?? throw new ArgumentNullException(nameof(persistenceConfigurer));
            return this;
        }


        Configuration ApplyCustomSettings(Configuration cfg)
        {
            var fluentConfig = Fluently.Configure(cfg);
            if (_persistenceConfigurer != null)
            {
                fluentConfig.Database(_persistenceConfigurer);
            }

            fluentConfig.Mappings(m =>
            {
                foreach (var mappingAssembly in _mappingAssemblies)
                {
                    m.HbmMappings.AddFromAssembly(mappingAssembly);
                    m.FluentMappings.AddFromAssembly(mappingAssembly).Conventions.AddAssembly(mappingAssembly);
                }

                if (_autoPersistenceModel != null)
                {
                    m.AutoMappings.Add(_autoPersistenceModel);
                }
            });

            if (_useDataAnnotationValidators || ShouldExposeConfiguration())
            {
                fluentConfig.ExposeConfiguration(AddValidatorsAndExposeConfiguration);
            }
            return fluentConfig.BuildConfiguration();
        }

        void AddValidatorsAndExposeConfiguration(Configuration e)
        {
            if (_useDataAnnotationValidators)
            {
                e.EventListeners.PreInsertEventListeners = InsertFirst(e.EventListeners.PreInsertEventListeners, new DataAnnotationsEventListener());
                e.EventListeners.PreUpdateEventListeners = InsertFirst(e.EventListeners.PreUpdateEventListeners, new DataAnnotationsEventListener());
            }
            if (ShouldExposeConfiguration())
            {
                _exposeConfiguration(e);
            }
        }

        /// <summary>
        ///     Loads configuration from properties dictionary and from external file if available.
        /// </summary>
        /// <returns></returns>
        Configuration LoadExternalConfiguration()
        {
            var cfg = new Configuration();
            if (_properties != null && _properties.Any())
            {
                cfg.AddProperties(_properties);
            }
            if (!string.IsNullOrEmpty(_configFile))
            {
                return cfg.Configure(_configFile);
            }
            if (File.Exists(DefaultNHibernateConfigFileName))
            {
                return cfg.Configure();
            }
            return cfg;
        }

        static T[] InsertFirst<T>(T[] array, T item)
        {
            if (array == null)
            {
                return new[] {item};
            }

            var items = new List<T>(array.Length + 1) {item};
            items.AddRange(array);
            return items.ToArray();
        }

        static string MakeLoadReadyAssemblyName(string assemblyName)
        {
            return (assemblyName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase) == -1)
                ? assemblyName.Trim() + ".dll"
                : assemblyName.Trim();
        }
    }
}