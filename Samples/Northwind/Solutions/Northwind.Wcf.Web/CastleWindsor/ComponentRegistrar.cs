namespace Northwind.Wcf.Web.CastleWindsor
{
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
        }

        private static void AddCustomRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                AllTypes.Pick().FromAssemblyNamed("Northwind.Infrastructure").WithService.DefaultInterface());
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
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
            // Since the TerritoriesService.svc must be associated with a concrete class,
            // we must register the concrete implementation here as the service
            container.AddComponent("territoriesWcfService", typeof(TerritoriesWcfService));
        }
    }
}