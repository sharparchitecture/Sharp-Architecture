namespace SharpArch.NHibernate.Extensions.DependencyInjection
{
    using System;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using Infrastructure.Logging;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;


    /// <summary>
    ///     NHibernate supporting infrastructure registration helpers.
    /// </summary>
    [PublicAPI]
    public static class NHibernateRegistrationExtensions
    {
        /// <summary>
        ///     Adds NHibernate classes required to support <see cref="NHibernateRepository{T,TId}" />,
        ///     <see cref="LinqRepository{TEntity,TId}" /> instantiation from container.
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
        /// <param name="configureSessionFactory">
        ///     NHibernate session factory configuration.
        ///     Function should return <see cref="NHibernateSessionFactoryBuilder" /> instance,
        ///     <see cref="IServiceProvider" /> is passed to allow retrieval of configuration.
        /// </param>
        /// <param name="sessionConfigurator">Optional callback to configure new session options.</param>
        /// <param name="statelessSessionConfigurator">Optional callback to configure new stateless session options.</param>
        /// <returns>
        ///     <paramref name="services" />
        /// </returns>
        public static IServiceCollection AddNHibernateWithSingleDatabase(
            this IServiceCollection services, Func<IServiceProvider, NHibernateSessionFactoryBuilder> configureSessionFactory,
            Func<ISessionBuilder, IServiceProvider, ISession>? sessionConfigurator = null,
            Func<IStatelessSessionBuilder, IServiceProvider, IStatelessSession>? statelessSessionConfigurator = null
        )
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureSessionFactory == null) throw new ArgumentNullException(nameof(configureSessionFactory));

            services.AddSingleton(sp =>
            {
                var logger = GetLogger(sp);
                logger.Debug?.Log("Building session factory...");

                var sfBuilder = configureSessionFactory(sp);
                var sessionFactory = sfBuilder.BuildSessionFactory();

                logger.Information?.Log("Build session factory {SessionFactoryId}", sessionFactory.GetHashCode());
                return sessionFactory;
            });

            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                ISession session = sessionConfigurator == null
                    ? sessionFactory.OpenSession()
                    : sessionConfigurator(sessionFactory.WithOptions(), sp);

                GetLogger(sp).Debug?.Log("Created Session {SessionId}", session.GetSessionImplementation().SessionId);

                return session;
            });

            services.AddScoped(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                IStatelessSession session = statelessSessionConfigurator == null
                    ? sessionFactory.OpenStatelessSession()
                    : statelessSessionConfigurator(sessionFactory.WithStatelessOptions(), sp);

                GetLogger(sp).Debug?.Log("Created stateless Session {SessionId}", session.GetSessionImplementation().SessionId);

                return session;
            });

            services.AddScoped<TransactionManager>();
            services.AddTransient<INHibernateTransactionManager>(sp => sp.GetRequiredService<TransactionManager>());
            services.AddTransient<ITransactionManager>(sp => sp.GetRequiredService<TransactionManager>());

            return services;
        }

        static LogWrapper GetLogger(IServiceProvider sp)
        {
            var logger = new LogWrapper(sp.GetRequiredService<global::Microsoft.Extensions.Logging.ILoggerFactory>()
                .CreateLogger("SharpArch.NHibernate.Extensions.DependencyInjection"));
            return logger;
        }
    }
}
