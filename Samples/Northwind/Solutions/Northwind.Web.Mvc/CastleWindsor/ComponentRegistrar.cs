namespace Northwind.Web.Mvc.CastleWindsor
{
    using Castle.Core;
    using Castle.Core.Configuration;
    using Castle.DynamicProxy;
    using Castle.Facilities.FactorySupport;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using Northwind.WcfServices;
    using Northwind.Web.Mvc.WcfServices;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Contracts.Repositories;
    using SharpArch.Web.Mvc.Castle;

    public class ComponentRegistrar
    {
        public static void AddComponentsTo(IWindsorContainer container)
        {
            AddGenericRepositoriesTo(container);
            AddCustomRepositoriesTo(container);
            AddWcfServiceFactoriesTo(container);
        }

        private static void AddCustomRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                AllTypes.Pick().FromAssemblyNamed("Northwind.Infrastructure").WithService.FirstNonGenericCoreInterface(
                    "Northwind.Domain"));

            container.Register(
                AllTypes.Pick().FromAssemblyNamed("Northwind.Tasks").WithService.FirstNonGenericCoreInterface(
                    "Northwind.Domain.Contracts.Tasks"));
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
            container.AddComponent(
                "entityDuplicateChecker", typeof(IEntityDuplicateChecker), typeof(EntityDuplicateChecker));
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

        private static void AddWcfServiceFactoriesTo(IWindsorContainer container)
        {
            container.AddFacility("factories", new FactorySupportFacility());
            container.AddComponent("standard.interceptor", typeof(StandardInterceptor));

            var factoryKey = "territoriesWcfServiceFactory";
            var serviceKey = "territoriesWcfService";

            container.AddComponent(factoryKey, typeof(TerritoriesWcfServiceFactory));
            var config = new MutableConfiguration(serviceKey);
            config.Attributes["factoryId"] = factoryKey;
            config.Attributes["factoryCreate"] = "Create";
            container.Kernel.ConfigurationStore.AddComponentConfiguration(serviceKey, config);
            container.Kernel.AddComponent(
                serviceKey, 
                typeof(ITerritoriesWcfService), 
                typeof(TerritoriesWcfServiceClient), 
                LifestyleType.PerWebRequest);
        }
    }
}