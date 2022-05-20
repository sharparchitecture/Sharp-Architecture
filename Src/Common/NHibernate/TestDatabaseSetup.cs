namespace SharpArch.Testing.NHibernate;

using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using FluentNHibernate.Automapping;
using global::NHibernate;
using global::NHibernate.Cfg;
using global::NHibernate.Tool.hbm2ddl;
using Infrastructure;
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
    readonly string _basePath;
    readonly Assembly[] _mappingAssemblies;
    readonly Type _persistenceModelGenerator;
    Configuration? _configuration;
    ISessionFactory? _sessionFactory;

    /// <summary>
    ///     Allows to apply custom configuration to new ISession.
    /// </summary>
    /// <remarks>
    ///     Callback is executed by default implementation of <see cref="ConfigureSession" /> from <see cref="InitializeSession" />.
    /// </remarks>
    public Action<ISessionBuilder>? SessionConfigurator { get; set; }

    /// <summary>
    ///     Allows to apply custom configuration to new ISession.
    /// </summary>
    /// <remarks>
    ///     Callback is executed by default implementation of <see cref="ConfigureSession" /> from <see cref="InitializeSession" />.
    /// </remarks>
    public Action<IStatelessSessionBuilder>? StatelessSessionConfigurator { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestDatabaseSetup" /> class.
    /// </summary>
    /// <param name="basePath">Base bath to use when looking for mapping assemblies and default NHibernate configuration file.</param>
    /// <param name="persistenceModelGenerator">Persistence model generator, type. Must implement <see cref="IAutoPersistenceModelGenerator" />.</param>
    /// <param name="mappingAssemblies">
    ///     List of assemblies containing NHibernate mapping files and persistence model generator.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="basePath" />, <paramref name="persistenceModelGenerator" /> or <paramref name="mappingAssemblies" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="persistenceModelGenerator" /> does not implement <see cref="IAutoPersistenceModelGenerator" />.
    /// </exception>
    public TestDatabaseSetup(
        string basePath,
        Type persistenceModelGenerator,
        Assembly[] mappingAssemblies)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _persistenceModelGenerator = persistenceModelGenerator ?? throw new ArgumentNullException(nameof(persistenceModelGenerator));
        if (!typeof(IAutoPersistenceModelGenerator).IsAssignableFrom(persistenceModelGenerator))
            throw new ArgumentException($"Type {persistenceModelGenerator.FullName} must implement {nameof(IAutoPersistenceModelGenerator)}.");

        if (mappingAssemblies == null) throw new ArgumentNullException(nameof(mappingAssemblies));
        _mappingAssemblies = mappingAssemblies.Distinct().ToArray();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestDatabaseSetup" /> class.
    /// </summary>
    /// <param name="persistenceModelGenerator">Persistence model generator, type. Must implement <see cref="IAutoPersistenceModelGenerator" />.</param>
    /// <param name="baseAssembly">
    ///     Assembly to use to determine configuration folder. Typically is it assembly containing
    ///     tests.
    /// </param>
    /// <param name="mappingAssemblies"></param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="baseAssembly" /> or <paramref name="mappingAssemblies" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="persistenceModelGenerator" /> does not implement <see cref="IAutoPersistenceModelGenerator" />.
    /// </exception>
    public TestDatabaseSetup(
        Assembly baseAssembly,
        Type persistenceModelGenerator,
        Assembly[] mappingAssemblies)
        : this(CodeBaseLocator.GetAssemblyCodeBasePath(baseAssembly),
            persistenceModelGenerator,
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
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Returns NHibernate <see cref="Configuration" />.
    ///     Configuration instance is cached, all subsequent calls will return the same instance.
    /// </summary>
    public Configuration GetConfiguration()
    {
        if (_configuration != null) return _configuration;

        var autoPersistenceModel = GenerateAutoPersistenceModel();

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
    ///     Generates auto-persistence model.
    /// </summary>
    /// <returns>
    ///     <see cref="AutoPersistenceModel" />
    /// </returns>
    /// <exception cref="TargetInvocationException">Unable to instantiate AutoPersistenceModelGenerator.</exception>
    public AutoPersistenceModel GenerateAutoPersistenceModel()
    {
        var generator = (IAutoPersistenceModelGenerator)Activator.CreateInstance(_persistenceModelGenerator)!;
        return generator.Generate();
    }

    /// <summary>
    ///     Override this method to customize NHibernate configuration.
    /// </summary>
    /// <param name="builder">
    ///     <see cref="NHibernateSessionFactoryBuilder" />
    /// </param>
    protected virtual void Customize(NHibernateSessionFactoryBuilder builder)
    {
    }

    /// <summary>
    ///     Configures <see cref="ISession" /> before usage.
    /// </summary>
    /// <param name="sessionBuilder">Session builder.</param>
    protected virtual ISessionBuilder ConfigureSession(ISessionBuilder sessionBuilder)
    {
        if (sessionBuilder == null) throw new ArgumentNullException(nameof(sessionBuilder));
        SessionConfigurator?.Invoke(sessionBuilder);
        return sessionBuilder;
    }

    /// <summary>
    ///     Configures <see cref="IStatelessSession" /> before usage.
    /// </summary>
    /// <param name="statelessSessionBuilder">Session builder.</param>
    protected virtual IStatelessSessionBuilder ConfigureStatelessSession(IStatelessSessionBuilder statelessSessionBuilder)
    {
        if (statelessSessionBuilder == null) throw new ArgumentNullException(nameof(statelessSessionBuilder));
        StatelessSessionConfigurator?.Invoke(statelessSessionBuilder);
        return statelessSessionBuilder;
    }

    /// <summary>
    ///     Returns NHibernate <see cref="ISessionFactory" />.
    ///     Session factory instance is cached, all subsequent calls to GetSessionFactory() will return the same instance.
    /// </summary>
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
    public ISession InitializeSession()
    {
        var sessionBuilder = GetSessionFactory().WithOptions();
        var session = ConfigureSession(sessionBuilder).OpenSession();
        var connection = session.Connection;
        new SchemaExport(_configuration).Execute(false, true, false, connection, null);
        return session;
    }

    /// <summary>
    ///     Creates new NHibernate stateless session.
    /// </summary>
    /// <remarks>
    ///     When testing using transient database, database structure should be created once per test.
    ///     If stateless session is required, it should be created using connection opened by regular session, <see cref="InitializeSession" />.
    /// </remarks>
    /// <returns>NHibernate Session</returns>
    public IStatelessSession CreateStatelessSessionForConnection(DbConnection connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        var sessionBuilder = GetSessionFactory().WithStatelessOptions();
        sessionBuilder.Connection(connection);
        var statelessSession = ConfigureStatelessSession(sessionBuilder).OpenStatelessSession();
        return statelessSession;
    }

    /// <summary>
    ///     Closes the specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    public static void Close(ISession? session)
        => session?.Dispose();

    /// <summary>
    ///     Shutdowns the specified session factory.
    /// </summary>
    /// <param name="sessionFactory">The session factory.</param>
    /// <remarks>
    ///     Disposing <see cref="TestDatabaseSetup" /> will destroy Session Factory associated with this instance.
    /// </remarks>
    public static void Shutdown(ISessionFactory? sessionFactory)
        => sessionFactory?.Dispose();
}
