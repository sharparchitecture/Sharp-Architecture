namespace SharpArch.NHibernate.MultiDb
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using Impl;
    using JetBrains.Annotations;


    /// <summary>
    ///     Ad-hoc callback to configure parameters of new session.
    /// </summary>
    /// <remarks>
    ///     It will be called every time sessions is created.
    /// </remarks>
    /// <param name="databaseIdentifier">Database identifier.</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" /> instance. Service provider will have same scope as session (e.g. per-request).
    ///     It can be used to resolve dependencies required for interceptors to be injected into session.
    /// </param>
    /// <param name="sessionBuilder">Session builder.</param>
    public delegate void ConfigureSession(string databaseIdentifier, IServiceProvider serviceProvider, ISessionBuilder sessionBuilder);


    /// <summary>
    ///     Ad-hoc callback to configure parameters of new stateless session.
    /// </summary>
    /// <remarks>
    ///     It will be called every time sessions is created.
    /// </remarks>
    /// <param name="databaseIdentifier">Database identifier.</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" /> instance. Service provider will have same scope as session (e.g. per-request).
    ///     It can be used to resolve dependencies required for interceptors to be injected into session.
    /// </param>
    /// <param name="sessionBuilder">Session builder.</param>
    public delegate void ConfigureStatelessSession(
        string databaseIdentifier, IServiceProvider serviceProvider, IStatelessSessionBuilder sessionBuilder);


    /// <summary>
    ///     Manages Sessions in given scope
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>Requirements:</description>
    ///         </listheader>
    ///         <item>
    ///             <description>Keep track of open sessions.</description>
    ///         </item>
    ///         <item>
    ///             <description>Ensure ISession is created one time only.</description>
    ///         </item>
    ///         <item>
    ///             <description>Dispose all created Sessions.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Stateless sessions are not tracked as there is no benefit of using shared instance if
    ///                 <see cref="IStatelessSession" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class NHibernateSessionRegistry : IDisposable, ISessionRegistry, INHibernateSessionRegistry
    {
        readonly InitializationParams _initializationParams;

        readonly ConcurrentDictionary<string, INHibernateTransactionManager> _sessions =
            new ConcurrentDictionary<string, INHibernateTransactionManager>(4, 4);

        public NHibernateSessionRegistry(
            [NotNull] NHibernateSessionFactoryRegistry sessionFactoryRegistry,
            [NotNull] IServiceProvider serviceProvider,
            ConfigureSession configureSession = null,
            ConfigureStatelessSession configureStatelessSession = null)
        {
            if (sessionFactoryRegistry == null) throw new ArgumentNullException(nameof(sessionFactoryRegistry));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            _initializationParams = new InitializationParams(sessionFactoryRegistry, serviceProvider, configureSession, configureStatelessSession);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var transactionManager in _sessions.Values) transactionManager.Session.Dispose();
        }

        /// <inheritdoc />
        public INHibernateTransactionManager GetNHibernateTransactionManager(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));

            if (!_sessions.TryGetValue(databaseIdentifier, out var transactionManager))
                transactionManager = _sessions.GetOrAdd(databaseIdentifier, key =>
                {
                    var sessionFactory = _initializationParams.SessionFactoryRegistry.GetSessionFactory(key);

                    var builder = sessionFactory.WithOptions();

                    _initializationParams.ConfigureSession?.Invoke(key, _initializationParams.ServiceProvider, builder);
                    var session = builder.OpenSession();
                    return new NHibernateTransactionManager(session);
                });
            return transactionManager;
        }

        /// <inheritdoc />
        public IStatelessSession CreateStatelessSession(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            var builder = _initializationParams.SessionFactoryRegistry.GetSessionFactory(databaseIdentifier).WithStatelessOptions();
            _initializationParams.ConfigureStatelessSession?.Invoke(databaseIdentifier, _initializationParams.ServiceProvider, builder);
            return builder.OpenStatelessSession();
        }

        /// <inheritdoc />
        public KeyValuePair<string, ITransactionManager>[] GetExistingTransactionsSnapshot()
        {
            var openSessions = _sessions.Select(x => new KeyValuePair<string, ITransactionManager>(x.Key, x.Value)).ToArray();

            // todo: verify if Array.Empty can be omitted.
            return openSessions.Length == 0
                ? Array.Empty<KeyValuePair<string, ITransactionManager>>()
                : openSessions;
        }

        /// <inheritdoc />
        public bool ContainsDatabase(string databaseIdentifier)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public ITransactionManager GetTransactionManager(string databaseIdentifier)
            => GetNHibernateTransactionManager(databaseIdentifier);


        class InitializationParams
        {
            public NHibernateSessionFactoryRegistry SessionFactoryRegistry { get; }
            public IServiceProvider ServiceProvider { get; }
            public ConfigureSession ConfigureSession { get; }
            public ConfigureStatelessSession ConfigureStatelessSession { get; }

            public InitializationParams(
                [NotNull] NHibernateSessionFactoryRegistry sessionFactoryRegistry, IServiceProvider serviceProvider, ConfigureSession configureSession,
                ConfigureStatelessSession configureStatelessSession)
            {
                SessionFactoryRegistry = sessionFactoryRegistry;
                ServiceProvider = serviceProvider;
                ConfigureSession = configureSession;
                ConfigureStatelessSession = configureStatelessSession;
            }
        }
    }
}
