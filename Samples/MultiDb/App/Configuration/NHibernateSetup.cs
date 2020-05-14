namespace MultiDatabase.Sample.Configuration
{
    using System;
    using System.Linq;
    using Autofac;
    using FluentNHibernate.Cfg.Db;
    using LogDb;
    using MainDb;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Persistence;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate.Configuration;
    using SharpArch.NHibernate.Extensions.DependencyInjection;
    using SharpArch.NHibernate.Impl;
    using SharpArch.NHibernate.MultiDb;


    public static class NHibernateSetup
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {

            services.AddNHibernateDatabases(ConfigureSessionFactoryRegistry);

            services.AddScoped<NHibernateSessionRegistry>(sp => new NHibernateSessionRegistry(sp.GetRequiredService<NHibernateSessionFactoryRegistry>(), sp));

            services.AddTransient<ISessionRegistry>(sp => sp.GetRequiredService<NHibernateSessionRegistry>());
            services.AddTransient<INHibernateSessionRegistry>(sp => sp.GetRequiredService<NHibernateSessionRegistry>());
            services.AddSingleton<IDatabaseIdentifierProvider>(new AttributeBasedDatabaseIdentifierProvider());

            return services;
        }

        static void ConfigureSessionFactoryRegistry(IServiceProvider serviceProvider, INHibernateSessionFactoryRegistryBuilder builder) 
        {
            builder.Add(Databases.Log, new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(new[] {typeof(LogDbPersistenceModelGenerator).Assembly})
                .UsePersistenceConfigurer(
                    MsSqlConfiguration.MsSql2012.ConnectionString("Server=localhost,2433;Database=Log;User Id=sa;Password=Password12!;")
                )
                .UseAutoPersistenceModel(new MainDbPersistenceModelGenerator().Generate())
            );

            builder.Add(Databases.Main, new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(new[] {typeof(MainDbPersistenceModelGenerator).Assembly})
                .UsePersistenceConfigurer(
                    MsSqlConfiguration.MsSql2012.ConnectionString("Server=localhost,2433;Database=Log;User Id=sa;Password=Password12!;")
                )
                .UseAutoPersistenceModel(new MainDbPersistenceModelGenerator().Generate())
            );
        }

        public static void AddRepositories(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(NHibernateRepository<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
