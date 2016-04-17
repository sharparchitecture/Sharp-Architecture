
namespace Northwind.Wcf.Web.CastleWindsor
{
    using System;
    using Castle.Facilities.WcfIntegration;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using Northwind.WcfServices;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Contracts.Repositories;
    
    public class ComponentRegistrar
    {
        public static void AddComponentsTo(IWindsorContainer container)
        {
            AddGenericRepositoriesTo(container);
            AddCustomRepositoriesTo(container);
            AddWcfServicesTo(container);
            container.Install(new NHibernateInstaller());
        }

        private static void AddCustomRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                AllTypes.Pick().FromAssemblyNamed("Northwind.Infrastructure").WithService.DefaultInterfaces());
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
            container.Register(Component.For<ITransactionManager>()
                .ImplementedBy<TransactionManager>());
            container.AddComponent(
                "sessionFactoryKeyProvider", typeof(ISessionFactoryKeyProvider), typeof(DefaultSessionFactoryKeyProvider));
            container.AddComponent("repositoryType", typeof(IRepository<>), typeof(NHibernateRepository<>));
            container.AddComponent(
                "nhibernateRepositoryType", typeof(INHibernateRepository<>), typeof(NHibernateRepository<>));
            container.AddComponent(
                "repositoryWithTypedId", typeof(IRepositoryWithTypedId<,>), typeof(NHibernateRepositoryWithTypedId<,>));
            container.AddComponent(
                "nhibernateRepositoryWithTypedId", 
                typeof(INHibernateRepositoryWithTypedId<,>), 
                typeof(NHibernateRepositoryWithTypedId<,>));
        }

        private static void AddWcfServicesTo(IWindsorContainer container)
        {
            container.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
                .Register(Component.For<ITerritoriesWcfService>()
                    .ImplementedBy<TerritoriesWcfService>()
                    .ActAs(new DefaultServiceModel()));
        }
    }
}