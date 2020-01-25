using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using JetBrains.Annotations;
using NHibernate;
using SharpArch.Infrastructure.Caching;
using SharpArch.NHibernate.NHibernateValidator;

namespace SharpArch.NHibernate.Configuration
{
    /// <summary>
    ///     Creates NHibernate SessionFactory <see cref="ISessionFactory" />
    /// </summary>
    /// <remarks>
    ///     Transient object, session factory must be registered as singleton in DI Container.
    /// </remarks>
    /// <threadsafety static="false" instance="false" />
    [PublicAPI]
    public class NHibernateSessionFactoryBuilder : INHibernateSessionFactoryBuilder
    {
        /// <summary>
        ///     Default configuration file name.
        /// </summary>
        public const string DefaultNHibernateConfigFileName = @"hibernate.cfg.xml";

        readonly List<Assembly> _mappingAssemblies;
        List<string> _additionalDependencies;

        AutoPersistenceModel _autoPersistenceModel;
        string _configFile;

        [NotNull] INHibernateConfigurationCache _configurationCache;
        Action<global::NHibernate.Cfg.Configuration> _exposeConfiguration;
        IPersistenceConfigurer _persistenceConfigurer;
        IDictionary<string, string> _properties;
        global::NHibernate.Cfg.Configuration _cachedConfiguration;

        bool _useDataAnnotationValidators;
        Action<CacheSettingsBuilder> _cacheSettingsBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NHibernateSessionFactoryBuilder" /> class.
        /// </summary>
        public NHibernateSessionFactoryBuilder()
        {
            _configurationCache = NullNHibernateConfigurationCache.Null;
            _mappingAssemblies = new List<Assembly>(8);
            _additionalDependencies = new List<string>(8);
        }

        /// <summary>
        ///     Creates the session factory.
        /// </summary>
        /// <returns> NHibernate session factory <see cref="ISessionFactory" />.</returns>
        /// <exception cref="T:System.InvalidOperationException">No dependencies were specified</exception>
        [NotNull]
        public ISessionFactory BuildSessionFactory()
        {
            var configuration = BuildConfiguration();
            return configuration.BuildSessionFactory();
        }

        /// <summary>
        ///     Builds NHibernate configuration.
        /// </summary>
        /// <param name="basePath">
        ///     Base directory to use for loading additional files.
        ///     If <c>null</c> base folder of the current assembly is used.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         Any changes made to configuration object after this point <b>will not be persisted</b> in configuration cache.
        ///         This can be useful to make dynamic changes to configuration or in case changes cannot be serialized
        ///         (e.g. event listeners are not marked with <see cref="System.SerializableAttribute" />.
        ///     </para>
        ///     <para>
        ///         To make persistent changes use <seealso cref="ExposeConfiguration" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">No dependencies were specified for configuration cache using
        /// <see cref="AddMappingAssemblies"/>, <see cref="UseConfigFile(string)"/> or <see cref="WithFileDependency(string)"/>.
        /// </exception>
        [NotNull]
        public global::NHibernate.Cfg.Configuration BuildConfiguration(string basePath = null)
        {
            if (_cachedConfiguration != null) return _cachedConfiguration;

            global::NHibernate.Cfg.Configuration configuration = null;
            DateTime? timestamp = null;
            if (_configurationCache != NullNHibernateConfigurationCache.Null)
            {
                var dependencyList = basePath == null
                    ? DependencyList.WithBasePathOfAssembly(Assembly.GetExecutingAssembly())
                    : DependencyList.WithPathPrefix(basePath);
                dependencyList
                    .AddAssemblies(_mappingAssemblies);

                if (!string.IsNullOrEmpty(_configFile)) dependencyList.AddFile(_configFile);
                dependencyList.AddFiles(_additionalDependencies);

                timestamp = dependencyList.GetLastModificationTime();
                if (timestamp == null) throw new InvalidOperationException($"No dependencies for configuration cache were specified. "+
                    $"Use {nameof(AddMappingAssemblies)}(), {nameof(UseConfigFile)}() or {nameof(WithFileDependency)}() to specify dependencies.");

                configuration = _configurationCache.TryLoad(timestamp.Value);
            }

            if (configuration == null)
            {
                configuration = LoadExternalConfiguration();
                configuration = ApplyCustomSettings(configuration);
                if (_configurationCache != NullNHibernateConfigurationCache.Null && timestamp.HasValue)
                    _configurationCache.Save(configuration, timestamp.Value);
            }

            _cachedConfiguration = configuration;
            return configuration;
        }

        /// <summary>
        ///     Allows to alter configuration before creating NHibernate configuration.
        /// </summary>
        /// <remarks>
        ///     Changes to configuration will be persisted in configuration cache, if it is enabled.
        ///     In case changes must not be persisted in cache, they must be applied after <seealso cref="BuildConfiguration" />.
        /// </remarks>
        [NotNull]
        public NHibernateSessionFactoryBuilder ExposeConfiguration([NotNull] Action<global::NHibernate.Cfg.Configuration> config)
        {
            _exposeConfiguration = config ?? throw new ArgumentNullException(nameof(config));
            return this;
        }

        bool ShouldExposeConfiguration()
        {
            return _exposeConfiguration != null;
        }

        /// <summary>
        ///     Allows to cache compiled NHibernate configuration.
        /// </summary>
        /// <param name="configurationCache">The configuration cache, see <see cref="INHibernateConfigurationCache" />. </param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Please provide configuration cache instance.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseConfigurationCache(
            [NotNull] INHibernateConfigurationCache configurationCache)
        {
            _configurationCache = configurationCache ??
                throw new ArgumentNullException(nameof(configurationCache), "Please provide configuration cache instance.");
            return this;
        }

        /// <summary>
        ///     Allows to specify additional assemblies containing FluentNHibernate mappings.
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
        ///     Add generic file dependency.
        ///     Used with session cache to add dependency which is not used to configure session
        ///     (e.g. application configuration, shared library, etc...)
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">File name cannot be empty</exception>
        public NHibernateSessionFactoryBuilder WithFileDependency([NotNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be empty", nameof(fileName));
            }

            _additionalDependencies.Add(fileName);
            return this;
        }

        /// <summary>
        ///     Allows to specify FluentNhibernate auto-persistence model to use..
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
        ///     Allows to specify additional NHibernate properties, see
        ///     http://nhibernate.info/doc/nhibernate-reference/session-configuration.html.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Builder instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="properties" /> is <c>null</c>.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseProperties([NotNull] IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (_properties == null)
                _properties = new Dictionary<string, string>(64);
            foreach (var pair in properties)
            {
                _properties[pair.Key] = pair.Value;
            }
            return this;
        }

        /// <summary>
        ///     Allows to use Data Annotations and <see cref="Validator" /> to validate entities before insert/update.
        /// </summary>
        /// <remarks>
        ///     See https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations%28v=vs.110%29.aspx for details
        ///     about Data Annotations.
        /// </remarks>
        /// <seealso cref="DataAnnotationsEventListener" />.
        [NotNull]
        public NHibernateSessionFactoryBuilder UseDataAnnotationValidators(bool addDataAnnotationValidators)
        {
            _useDataAnnotationValidators = addDataAnnotationValidators;
            return this;
        }

        /// <summary>
        ///     Allows to specify nhibernate configuration file.
        /// </summary>
        /// <remarks>
        ///     See http://nhibernate.info/doc/nhibernate-reference/session-configuration.html#configuration-xmlconfig for more
        ///     details
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
        /// Allows to configure second-level cache.
        /// </summary>
        /// <param name="cacheSettingsBuilder">Cache settings configuration. Use <c>null</c> to clear previous setting.</param>
        [NotNull]
        public NHibernateSessionFactoryBuilder UseCache([CanBeNull] Action<CacheSettingsBuilder> cacheSettingsBuilder)
        {
            _cacheSettingsBuilder = cacheSettingsBuilder;
            return this;
        }

        /// <summary>
        ///     Allows to specify custom configuration using <see cref="IPersistenceConfigurer" />.
        /// </summary>
        /// <param name="persistenceConfigurer">The persistence configurer.</param>
        /// <example>
        ///     <code>
        /// var persistenceConfigurer =
        ///   SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));
        /// var configuration = new NHibernateSessionFactoryBuilder()
        ///   .UsePersistenceConfigurer(persistenceConfigurer);
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException"><paramref name="persistenceConfigurer" /> is <c>null</c>.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder UsePersistenceConfigurer(
            [NotNull] IPersistenceConfigurer persistenceConfigurer)
        {
            _persistenceConfigurer = persistenceConfigurer ?? throw new ArgumentNullException(nameof(persistenceConfigurer));
            return this;
        }

        global::NHibernate.Cfg.Configuration ApplyCustomSettings(global::NHibernate.Cfg.Configuration cfg)
        {
            var fluentConfig = Fluently.Configure(cfg);
            if (_persistenceConfigurer != null)
            {
                fluentConfig.Database(_persistenceConfigurer);
            }

            if (_cacheSettingsBuilder != null)
            {
                fluentConfig.Cache(_cacheSettingsBuilder);
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

        void AddValidatorsAndExposeConfiguration(global::NHibernate.Cfg.Configuration e)
        {
            if (_useDataAnnotationValidators)
            {
                var dataAnnotationsEventListener = new DataAnnotationsEventListener();
                e.EventListeners.PreInsertEventListeners = InsertFirst(e.EventListeners.PreInsertEventListeners, dataAnnotationsEventListener);
                e.EventListeners.PreUpdateEventListeners = InsertFirst(e.EventListeners.PreUpdateEventListeners, dataAnnotationsEventListener);
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
        global::NHibernate.Cfg.Configuration LoadExternalConfiguration()
        {
            var cfg = new global::NHibernate.Cfg.Configuration();
            if (_properties != null && _properties.Count > 0)
            {
                cfg.AddProperties(_properties);
            }

            if (!string.IsNullOrEmpty(_configFile)
                && !string.Equals(_configFile, DefaultNHibernateConfigFileName, StringComparison.OrdinalIgnoreCase))
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
            return assemblyName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase) == -1
                ? assemblyName.Trim() + ".dll"
                : assemblyName.Trim();
        }
    }
}
