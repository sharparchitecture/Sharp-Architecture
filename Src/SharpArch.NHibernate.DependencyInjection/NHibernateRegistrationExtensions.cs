namespace SharpArch.NHibernate.Extensions.DependencyInjection
{
    using System;
    using Configuration;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using Impl;
    using Infrastructure.Logging;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using MultiDb;


    /// <summary>
    ///     NHibernate supporting infrastructure registration helpers.
    /// </summary>
    [PublicAPI]
    public static class NHibernateRegistrationExtensions
    {
        static readonly ILog _log = LogProvider.GetLogger("SharpArch.NHibernate.Extensions.DependencyInjection");

        /// <summary>
        ///     Adds NHibernate classes required to support <see cref="NHibernateRepository{TEntity,TId}" />,
        ///     <see cref="NHibernateRepository{T,TId}" /> and <see cref="LinqRepository{T,TId}" />  instantiation from container.
        ///     <para>
        ///         <see cref="ISessionFactory" /> and <see cref="ITransactionManager" /> are registered as Singleton.
        ///     </para>
        ///     <para>
        ///         <see cref="ISession" /> is registered as Scoped (e.g. per Http request for ASP.NET Core)
        ///     </para>
        ///     <para>
        ///         <see cref="IStatelessSession" /> is transient. Since it does not tracks state, there is no reason to share it.
        ///         Stateless session must be disposed by caller
        ///         as soon as it is not used anymore.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     Repository registration needs to be done separately.
        /// </remarks>
        /// <param name="services">Service collection.</param>
        /// <param name="sessionFactoryRegistryBuilder">
        ///     NHibernate session factory configuration.
        ///     Function should return <see cref="NHibernateSessionFactoryBuilder" /> instance,
        ///     <see cref="IServiceProvider" /> is passed to allow retrieval of configuration.
        /// </param>
        /// <param name="sessionConfigurator">Optional callback to configure new session options.</param>
        /// <param name="statelessSessionConfigurator">Optional callback to configure new stateless session options.</param>
        /// <returns>
        ///     <paramref name="services" />
        /// </returns>
        public static IServiceCollection AddNHibernateDatabases(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<IServiceProvider, INHibernateSessionFactoryRegistryBuilder> sessionFactoryRegistryBuilder,
            [CanBeNull] Func<IServiceProvider, ISessionBuilder, ISession> sessionConfigurator = null,
            [CanBeNull] Func<IServiceProvider, IStatelessSessionBuilder, IStatelessSession> statelessSessionConfigurator = null
        )
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (sessionFactoryRegistryBuilder == null) throw new ArgumentNullException(nameof(sessionFactoryRegistryBuilder));

            services.TryAddSingleton<IDatabaseIdentifierProvider>(new AttributeBasedDatabaseIdentifierProvider());

            services.AddSingleton(sp =>
            {
                _log.Debug("Building session factory registry...");
                var sessionFactoryRegistry = new NHibernateSessionFactoryRegistry();
                sessionFactoryRegistryBuilder(sp, sessionFactoryRegistry);

                _log.Info("Built session factory");
                return sessionFactoryRegistry;
            });

            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                ISession session = sessionConfigurator == null
                    ? sessionFactory.OpenSession()
                    : sessionConfigurator(sp, sessionFactory.WithOptions());

                if (_log.IsDebugEnabled()) _log.Debug("Created Session {SessionId}", session.GetSessionImplementation().SessionId);

                return session;
            });

            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                IStatelessSession session = statelessSessionConfigurator == null
                    ? sessionFactory.OpenStatelessSession()
                    : statelessSessionConfigurator(sp, sessionFactory.WithStatelessOptions());

                if (_log.IsDebugEnabled()) _log.Debug("Created stateless Session {SessionId}", session.GetSessionImplementation().SessionId);

                return session;
            });
            return services;
        }
    }
}
