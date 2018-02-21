#if NETFULL
namespace SharpArch.Testing.NUnit.NHibernate
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
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.FluentNHibernate;

    /// <summary>
    ///     Performs NHibernate and database initialization.
    /// </summary>
    [PublicAPI]
    public class TestDatabaseInitializer : IDisposable
    {
        readonly string _basePath;
        readonly string _mappingAssemblies;
        Configuration _configuration;
        ISessionFactory _sessionFactory;
        static readonly char[] assemblySeparator = new[] {','};


        /// <summary>
        ///     Initializes a new instance of the <see cref="TestDatabaseInitializer" /> class.
        /// </summary>
        /// <param name="basePath">Base bath to use when looking for mapping assemblies and default NHibernate configuration file.</param>
        /// <param name="mappingAssemblies">
        ///     A comma delimited list of assemblies containing NHibernate mapping files. Including
        ///     '.dll' at the end of each is optional.
        /// </param>
        public TestDatabaseInitializer(string basePath, string mappingAssemblies)
        {
            _basePath = basePath;
            _mappingAssemblies = mappingAssemblies ?? string.Empty;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///     Method disposes SessionFactory.
        /// </summary>
        public void Dispose()
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
        [CLSCompliant(false)]
        public static AutoPersistenceModel GenerateAutoPersistenceModel([NotNull] string[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            return (from asmName in assemblies
                select TryLoadAssembly(asmName)
                into asm
                from asmType in asm.GetTypes()
                where typeof(IAutoPersistenceModelGenerator).IsAssignableFrom(asmType)
                select Activator.CreateInstance(asmType) as IAutoPersistenceModelGenerator
                into generator
                select generator.Generate()).FirstOrDefault();
        }


        /// <summary>
        ///     Returns list of assemblies containing NHibernate mappings.
        /// </summary>
        public string[] GetMappingAssemblies()
        {
            return GetMappingAssemblies(_basePath, _mappingAssemblies);
        }


        /// <summary>
        ///     Returns list of assemblies containing NHibernate mappings.
        /// </summary>
        /// <param name="basePath">Base path to prepend assembly names.</param>
        /// <param name="mappingAssemblies">
        ///     A comma delimited list of assemblies containing NHibernate mapping files. Including
        ///     '.dll' at the end of each is optional.
        /// </param>
        [NotNull]
        public static string[] GetMappingAssemblies(string basePath, [NotNull] string mappingAssemblies)
        {
            if (mappingAssemblies == null) throw new ArgumentNullException(nameof(mappingAssemblies));

            var assemblies =
                mappingAssemblies.Split(assemblySeparator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => EnsureDllExtension(s)).ToArray();

            for (int i = 0; i < assemblies.Length; i++) {
                assemblies[i] = Path.Combine(basePath, assemblies[i]);
            }

            return assemblies;
        }


        /// <summary>
        ///     Returns NHibernate <see cref="Configuration" />.
        ///     Configuration instance is cached, all subsequent calls will return the same instance.
        /// </summary>
        [NotNull]
        public Configuration GetConfiguration()
        {
            if (_configuration != null)
                return _configuration;

            var mappingAssemblies = GetMappingAssemblies();
            var autoPersistenceModel = GenerateAutoPersistenceModel(mappingAssemblies);

            var builder = new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(mappingAssemblies)
                .UseAutoPersistenceModel(autoPersistenceModel);

            var defaultConfigFilePath =
                Path.Combine(_basePath, NHibernateSessionFactoryBuilder.DefaultNHibernateConfigFileName);
            if (File.Exists(defaultConfigFilePath)) {
                Debug.WriteLine(
                    $"Found default configuration file {NHibernateSessionFactoryBuilder.DefaultNHibernateConfigFileName} in output folder. Loading configuration from '{defaultConfigFilePath}'.");
                builder.UseConfigFile(defaultConfigFilePath);
            }

            _configuration = builder.BuildConfiguration();
            return _configuration;
        }


        /// <summary>
        ///     Returns NHibernate <see cref="ISessionFactory" />.
        ///     Session factory instance is cached, all subsequent calls to GetSessionFactory() will return the same instance.
        /// </summary>
        [NotNull]
        public ISessionFactory GetSessionFactory()
        {
            if (_sessionFactory != null)
                return _sessionFactory;
            _sessionFactory = GetConfiguration().BuildSessionFactory();
            return _sessionFactory;
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
        ///     Dispose <see cref="TestDatabaseInitializer" /> will destroy Session Factory associated with this instance.
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
        private static Assembly TryLoadAssembly(string assemblyPath)
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        /// <summary>
        ///     Adds dll extension to assembly name if required.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        static string EnsureDllExtension(string assemblyName)
        {
            assemblyName = assemblyName.Trim();
            const string dllExtension = ".dll";
            if (!assemblyName.EndsWith(dllExtension, StringComparison.OrdinalIgnoreCase))
                assemblyName = string.Concat(assemblyName, dllExtension);

            return assemblyName;
        }
    }
}

#endif
