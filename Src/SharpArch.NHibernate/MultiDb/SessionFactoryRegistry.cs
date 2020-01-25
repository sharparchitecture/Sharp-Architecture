using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;
using NHibernate;

namespace SharpArch.NHibernate.MultiDb
{
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
    public class SessionFactoryRegistry : IDisposable, ISessionFactoryRegistry
    {
        public static readonly string DefaultdatabaseIdentifier = "default";

        readonly ConcurrentDictionary<string, Container> _sessionFactoryBuilders =
            new ConcurrentDictionary<string, Container>(4, 16, StringComparer.Ordinal);

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var container in _sessionFactoryBuilders.Values)
            {
                if (container.SessionFactory.IsValueCreated) container.SessionFactory.Value.Dispose();
            }
        }

        public void Add([NotNull] string databaseIdentifier, [NotNull] INHibernateSessionFactoryBuilder sessionFactoryBuilder)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            if (sessionFactoryBuilder == null) throw new ArgumentNullException(nameof(sessionFactoryBuilder));

            AddLazyFactory(databaseIdentifier, new Container(sessionFactoryBuilder));
        }

        public ISessionFactory GetSessionFactory([NotNull] string databaseIdentifier)
            => GetLazyFactory(databaseIdentifier).SessionFactory.Value;

        public global::NHibernate.Cfg.Configuration GetConfiguration(string databaseIdentifier)
            => GetLazyFactory(databaseIdentifier).Configuration.Value;

        public bool Contains(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            return _sessionFactoryBuilders.ContainsKey(databaseIdentifier);
        }

        public bool IsSessionFactoryCreated(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
            return GetLazyFactory(databaseIdentifier).SessionFactory.IsValueCreated;
        }

        void AddLazyFactory(string databaseIdentifier, Container container)
        {
            if (!_sessionFactoryBuilders.TryAdd(databaseIdentifier, container))
                throw new InvalidOperationException($"SessionFactory with databaseIdentifier '{databaseIdentifier}' already registered.")
                {
                    Data = {["SessiondatabaseIdentifier"] = databaseIdentifier}
                };
        }

        Container GetLazyFactory(string databaseIdentifier)
        {
            if (string.IsNullOrWhiteSpace(databaseIdentifier)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));

            if (!_sessionFactoryBuilders.TryGetValue(databaseIdentifier, out var lazyFactory))
                throw new InvalidOperationException($"SessionFactory with databaseIdentifier '{databaseIdentifier}' was not registered.")
                {
                    Data = {["SessiondatabaseIdentifier"] = databaseIdentifier}
                };
            return lazyFactory;
        }


        class Container
        {
            public Lazy<global::NHibernate.Cfg.Configuration> Configuration { get; }

            public Lazy<ISessionFactory> SessionFactory { get; }

            public Container(INHibernateSessionFactoryBuilder factoryBuilder)
            {
                Configuration = new Lazy<global::NHibernate.Cfg.Configuration>(() => factoryBuilder.BuildConfiguration(), LazyThreadSafetyMode.ExecutionAndPublication);
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
