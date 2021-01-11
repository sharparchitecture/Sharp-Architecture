namespace SharpArch.Testing.NHibernate
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using FluentNHibernate.Automapping;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::NHibernate.Tool.hbm2ddl;
    using Infrastructure;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.FluentNHibernate;


    /// <summary>
    ///     Performs NHibernate and database initialization.
    /// </summary>
    /// <remarks>
    ///     By default it looks for file <see cref="NHibernateSessionFactoryBuilder.DefaultNHibernateConfigFileName" />
    ///     to load configuration, if you prefer fluent configuration, override <see cref="Customize" /> method.
    /// </remarks>
    [PublicAPI]
    public class TestDatabaseSetup : IDisposable
    {
        [NotNull] readonly string _basePath;
        [NotNull] readonly Assembly[] _mappingAssemblies;
        [CanBeNull] Configuration _configuration;
        [CanBeNull] ISessionFactory _sessionFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestDatabaseSetup" /> class.
        /// </summary>
        /// <param name="basePath">Base bath to use when looking for mapping assemblies and default NHibernate configuration file.</param>
        /// <param name="mappingAssemblies">
        ///     List of assemblies containing NHibernate mapping files and persistence model generator.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="basePath" /> or <paramref name="mappingAssemblies" /> is
        ///     <c>null</c>.
        /// </exception>
        public TestDatabaseSetup([NotNull] string basePath, [NotNull] Assembly[] mappingAssemblies)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            if (mappingAssemblies == null) throw new ArgumentNullException(nameof(mappingAssemblies));
            _mappingAssemblies = mappingAssemblies.Distinct().ToArray();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestDatabaseSetup" /> class.
        /// </summary>
        /// <param name="baseAssembly">
        ///     Assembly to use to determine configuration folder. Typically is it assembly containing
        ///     tests.
        /// </param>
        /// <param name="mappingAssemblies"></param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="baseAssembly" /> or <paramref name="mappingAssemblies" /> is
        ///     <c>null</c>.
        /// </exception>
        public TestDatabaseSetup([NotNull] Assembly baseAssembly, [NotNull] Assembly[] mappingAssemblies)
            : this(CodeBaseLocator.GetAssemblyCodeBasePath(baseAssembly),
                mappingAssemblies)
        {
        }

        /// <summary>
        ///     Disposes SessionFactory.
        /// </summary>
        public virtual void Dispose()
        {
            Shutdown(_sessionFactory);
            _sessionFactory = null;
            _configuration = null;
        }

        /// <summary>
        ///     Generates auto-persistence model.
        /// </summary>
        /// <param name="assemblies">List of assemblies to look for auto-persistence model generators.</param>
        /// <returns>
        ///     <see cref="AutoPersistenceModel" />
        /// </returns>
        /// <remarks>
        ///     This method will load and scan assemblies for <see cref="IAutoPersistenceModelGenerator" />.
        ///     Only first generated model is returned.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="assemblies" /> is <see langword="null" /></exception>
        /// <exception cref="InvalidOperationException">
        ///     Only one implementation of <see cref="IAutoPersistenceModelGenerator" /> is
        ///     allowed.
        /// </exception>
        /// <exception cref="TargetInvocationException">Unable to instantiate AutoPersistenceModelGenerator.</exception>
        [NotNull]
        public static AutoPersistenceModel GenerateAutoPersistenceModel([NotNull] Assembly[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            var persistenceGeneratorTypes = assemblies.SelectMany(a =>
                    a.GetTypes().Where(t =>
                        !t.IsAbstract && typeof(IAutoPersistenceModelGenerator).IsAssignableFrom(t)))
                .ToArray();
            if (persistenceGeneratorTypes.Length > 1)
                throw new InvalidOperationException($"Found multiple classes implementing {nameof(IAutoPersistenceModelGenerator)}. " +
                    "Only one persistence model generator is supported.")
                {
                    Data =
                    {
                        [nameof(IAutoPersistenceModelGenerator) + "s"] = persistenceGeneratorTypes,
                        ["Assemblies"] = assemblies
                    }
                };
            if (persistenceGeneratorTypes.Length == 0)
                throw new InvalidOperationException($"No classes implementing {nameof(IAutoPersistenceModelGenerator)} were found. " +
                    $"{nameof(TestDatabaseSetup)} requires persistence model generator to create test database.")
                {
                    Data =
                    {
                        ["Assemblies"] = assemblies
                    }
                };
            var generator = (IAutoPersistenceModelGenerator) Activator.CreateInstance(persistenceGeneratorTypes[0]);
            return generator.Generate();
        }

        /// <summary>
        ///     Returns NHibernate <see cref="Configuration" />.
        ///     Configuration instance is cached, all subsequent calls will return the same instance.
        /// </summary>
        [NotNull]
        public Configuration GetConfiguration()
        {
            if (_configuration != null) return _configuration;

            var autoPersistenceModel = GenerateAutoPersistenceModel(_mappingAssemblies);

            var builder = new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(_mappingAssemblies)
                .UseAutoPersistenceModel(autoPersistenceModel);

            var defaultConfigFilePath =
                Path.Combine(_basePath, NHibernateSessionFactoryBuilder.DefaultNHibernateConfigFileName);
            if (File.Exists(defaultConfigFilePath))
            {
                Debug.WriteLine(
                    $"Found default configuration file {NHibernateSessionFactoryBuilder.DefaultNHibernateConfigFileName} in output folder. Loading configuration from '{defaultConfigFilePath}'.");
                builder.UseConfigFile(defaultConfigFilePath);
            }

            Customize(builder);
            _configuration = builder.BuildConfiguration();
            return _configuration;
        }

        /// <summary>
        ///     Override this method to customize NHibernate configuration.
        /// </summary>
        /// <param name="builder">
        ///     <see cref="NHibernateSessionFactoryBuilder" />
        /// </param>
        protected virtual void Customize([NotNull] NHibernateSessionFactoryBuilder builder)
        {
        }

        /// <summary>
        ///     Returns NHibernate <see cref="ISessionFactory" />.
        ///     Session factory instance is cached, all subsequent calls to GetSessionFactory() will return the same instance.
        /// </summary>
        [NotNull]
        public ISessionFactory GetSessionFactory()
        {
            if (_sessionFactory != null) return _sessionFactory;
            _sessionFactory = GetConfiguration().BuildSessionFactory();
            return _sessionFactory!;
        }

        /// <summary>
        ///     Creates new NHibernate session and initializes database structure.
        /// </summary>
        /// <returns>NHibernate Session</returns>
        [NotNull]
        public ISession InitializeSession()
        {
            var session = GetSessionFactory().OpenSession();
            var connection = session.Connection;
            new SchemaExport(_configuration).Execute(false, true, false, connection, null);

            return session;
        }

        /// <summary>
        ///     Closes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        public static void Close([CanBeNull] ISession session)
        {
            session?.Dispose();
        }

        /// <summary>
        ///     Shutdowns the specified session factory.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        /// <remarks>
        ///     Dispose <see cref="TestDatabaseSetup" /> will destroy Session Factory associated with this instance.
        /// </remarks>
        public static void Shutdown([CanBeNull] ISessionFactory sessionFactory)
        {
            sessionFactory?.Dispose();
        }

        /// <summary>
        ///     Loads the assembly.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        [NotNull]
        static Assembly TryLoadAssembly([NotNull] string assemblyPath)
            => Assembly.LoadFrom(assemblyPath);

        /// <summary>
        ///     Adds dll extension to assembly name if required.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        [NotNull]
        static string EnsureDllExtension([NotNull] string assemblyName)
        {
            assemblyName = assemblyName.Trim();
            const string dllExtension = ".dll";
            if (!assemblyName.EndsWith(dllExtension, StringComparison.OrdinalIgnoreCase)) assemblyName = string.Concat(assemblyName, dllExtension);

            return assemblyName;
        }
    }
}
