namespace SharpArch.NHibernate.MultiDb
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using JetBrains.Annotations;


    public interface INHibernateSessionFactoryRegistryBuilder
    {
        void Add(string databaseIdentifier, INHibernateSessionFactoryBuilder sessionFactoryBuilder);
    }

    /// <summary>
    ///     Contains registered NHibernate Factories.
    ///     <para>
    ///         Must be registered as singleton.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>Requirements:</description>
    ///         </listheader>
    ///         <item>
    ///             <description>Keep track of registered / initialized session factories.</description>
    ///         </item>
    ///         <item>
    ///             <description>Lazy initialization of factory.</description>
    ///         </item>
    ///         <item>
    ///             <description>Ensure factory is create one time only.</description>
    ///         </item>
    ///         <item>
    ///             <description>Dispose all created SessionFactory instances.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class NHibernateSessionFactoryRegistry : IDisposable, ISessionFactoryRegistry, INHibernateSessionFactoryRegistryBuilder
    {
        readonly ConcurrentDictionary<string, Container> _sessionFactoryBuilders =
            new ConcurrentDictionary<string, Container>(4, 16, StringComparer.Ordinal);

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var container in _sessionFactoryBuilders.Values)
                if (container.SessionFactory.IsValueCreated)
                    container.SessionFactory.Value.Dispose();
        }

        /// <inheritdoc />
        public void Add(string databaseIdentifier, INHibernateSessionFactoryBuilder sessionFactoryBuilder)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            if (sessionFactoryBuilder == null) throw new ArgumentNullException(nameof(sessionFactoryBuilder));

            AddLazyFactory(databaseIdentifier, new Container(sessionFactoryBuilder));
        }

        /// <inheritdoc />
        public ISessionFactory GetSessionFactory(string databaseIdentifier)
            => GetLazyFactory(databaseIdentifier).SessionFactory.Value;

        /// <inheritdoc />
        public Configuration GetConfiguration(string databaseIdentifier)
            => GetLazyFactory(databaseIdentifier).Configuration.Value;

        /// <inheritdoc />
        public bool Contains(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            return _sessionFactoryBuilders.ContainsKey(databaseIdentifier);
        }

        /// <inheritdoc />
        public bool IsSessionFactoryCreated(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            return GetLazyFactory(databaseIdentifier).SessionFactory.IsValueCreated;
        }

        void AddLazyFactory(string databaseIdentifier, Container container)
        {
            if (!_sessionFactoryBuilders.TryAdd(databaseIdentifier, container))
                throw new InvalidOperationException($"SessionFactory with databaseIdentifier '{databaseIdentifier}' already registered.")
                {
                    Data = {[DatabaseIdentifier.ParameterName] = databaseIdentifier}
                };
        }

        Container GetLazyFactory(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));

            if (!_sessionFactoryBuilders.TryGetValue(databaseIdentifier, out var lazyFactory))
                throw new InvalidOperationException($"SessionFactory with databaseIdentifier '{databaseIdentifier}' was not registered.")
                {
                    Data = {[DatabaseIdentifier.ParameterName] = databaseIdentifier}
                };
            return lazyFactory;
        }


        class Container
        {
            public Lazy<Configuration> Configuration { get; }

            public Lazy<ISessionFactory> SessionFactory { get; }

            public Container([NotNull] NHibernate.INHibernateSessionFactoryBuilder factoryBuilder)
            {
                Configuration = new Lazy<Configuration>(() => factoryBuilder.BuildConfiguration(), LazyThreadSafetyMode.ExecutionAndPublication);
                SessionFactory = new Lazy<ISessionFactory>(
                    () =>
                    {
                        // ensure configuration is created
                        var _ = Configuration.Value;
                        return factoryBuilder.BuildSessionFactory();
                    }
                );
            }
        }
    }
}
