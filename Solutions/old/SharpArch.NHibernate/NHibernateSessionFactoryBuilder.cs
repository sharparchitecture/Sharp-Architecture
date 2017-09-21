namespace SharpArch.NHibernate
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
    ///     Transient object, session factory must be redistered as singletone in DI Container.
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

        private INHibernateConfigurationCache configurationCache;
        
        private AutoPersistenceModel autoPersistenceModel;
        private string configFile;
        private readonly List<string> mappingAssemblies;
        private IPersistenceConfigurer persistenceConfigurer;
        private IDictionary<string, string> properties;
        private bool useDataAnnotationValidators;
        Action<Configuration> exposeConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateSessionFactoryBuilder"/> class.
        /// </summary>
        public NHibernateSessionFactoryBuilder()
        {
            this.configurationCache = NullNHibernateConfigurationCache.Null;
            this.mappingAssemblies = new List<string>();
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
        /// This can be useful to make dynamic changes to configuration or in case changes cannot be serialized (e.g. event listeners are not marked with <see cref="SerializableAttribute"/>.
        /// </para>
        /// <para>
        /// To make persistent changes use <seealso cref="ExposeConfiguration"/>.
        /// </para>
        /// </remarks>
        [NotNull]
        public Configuration BuildConfiguration()
        {
            var configuration = configurationCache.LoadConfiguration(DefaultConfigurationName, configFile, mappingAssemblies);

            if (configuration == null)
            {
                configuration = LoadExternalConfiguration();
                configuration = ApplyCustomSettings(configuration);
                configurationCache.SaveConfiguration(DefaultConfigurationName, configuration);
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
            if (config == null) throw new ArgumentNullException(nameof(config));

            this.exposeConfiguration = config;
            return this;
        }

        private bool ShouldExposeConfiguration()
        {
            return exposeConfiguration != null;
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
            if (configurationCache == null) throw new ArgumentNullException(nameof(configurationCache), "Please provide configuration cache instance.");
            this.configurationCache = configurationCache;
            return this;
        }

        /// <summary>
        /// Allows to specify additional assemblies containing FluentNHibernate mappings.
        /// </summary>
        /// <param name="mappingAssemblies">The mapping assemblies.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Mapping assemblies are not specified.</exception>
        [NotNull]
        public NHibernateSessionFactoryBuilder AddMappingAssemblies([NotNull] IEnumerable<string> mappingAssemblies)
        {
            if (mappingAssemblies == null) throw new ArgumentNullException(nameof(mappingAssemblies), "Mapping assemblies are not specified.");

            this.mappingAssemblies.AddRange(mappingAssemblies);
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
            if (autoPersistenceModel == null) throw new ArgumentNullException(nameof(autoPersistenceModel));
            
            this.autoPersistenceModel = autoPersistenceModel;
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
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            this.properties = properties;
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
            this.useDataAnnotationValidators = addDataAnnotatonValidators;
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
            
            configFile = nhibernateConfigFile;

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
            if (persistenceConfigurer == null) throw new ArgumentNullException(nameof(persistenceConfigurer));
            
            this.persistenceConfigurer = persistenceConfigurer;
            return this;
        }

        private Configuration ApplyCustomSettings(Configuration cfg)
        {
            var fluentConfig = Fluently.Configure(cfg);
            if (persistenceConfigurer != null)
            {
                fluentConfig.Database(persistenceConfigurer);
            }

            fluentConfig.Mappings(m =>
            {
                foreach (string mappingAssembly in this.mappingAssemblies)
                {
                    Assembly assembly = Assembly.LoadFrom(MakeLoadReadyAssemblyName(mappingAssembly));

                    m.HbmMappings.AddFromAssembly(assembly);
                    m.FluentMappings.AddFromAssembly(assembly).Conventions.AddAssembly(assembly);
                }

                if (autoPersistenceModel != null)
                {
                    m.AutoMappings.Add(autoPersistenceModel);
                }
            });

            if (this.useDataAnnotationValidators || ShouldExposeConfiguration())
            {
                fluentConfig.ExposeConfiguration(AddValidatorsAndExposeConfiguration);
            }
            return fluentConfig.BuildConfiguration();
        }

        private void AddValidatorsAndExposeConfiguration(Configuration e)
        {
            if (useDataAnnotationValidators)
            {
                e.EventListeners.PreInsertEventListeners = InsertFirst(e.EventListeners.PreInsertEventListeners, new DataAnnotationsEventListener());
                e.EventListeners.PreUpdateEventListeners = InsertFirst(e.EventListeners.PreUpdateEventListeners, new DataAnnotationsEventListener());
            }
            if (ShouldExposeConfiguration())
            {
                exposeConfiguration(e);
            }
        }

        /// <summary>
        ///     Loads configuration from properties dictionary and from external file if available.
        /// </summary>
        /// <returns></returns>
        private Configuration LoadExternalConfiguration()
        {
            var cfg = new Configuration();
            if (properties != null && properties.Any())
            {
                cfg.AddProperties(properties);
            }
            if (!string.IsNullOrEmpty(configFile))
            {
                return cfg.Configure(configFile);
            }
            if (File.Exists(DefaultNHibernateConfigFileName))
            {
                return cfg.Configure();
            }
            return cfg;
        }

        private static T[] InsertFirst<T>(T[] array, T item)
        {
            if (array == null)
            {
                return new[] {item};
            }

            var items = new List<T> {item};
            items.AddRange(array);
            return items.ToArray();
        }

        private static string MakeLoadReadyAssemblyName(string assemblyName)
        {
            return (assemblyName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase) == -1)
                ? assemblyName.Trim() + ".dll"
                : assemblyName.Trim();
        }
    }
}
