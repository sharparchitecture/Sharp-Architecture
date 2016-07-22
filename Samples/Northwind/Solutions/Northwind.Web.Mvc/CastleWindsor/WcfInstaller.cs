using Castle.Core;
using Castle.Core.Configuration;
using Castle.DynamicProxy;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Northwind.WcfServices;
using Northwind.Web.Mvc.WcfServices;

namespace Northwind.Web.Mvc.CastleWindsor
{
    public class WcfInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
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
            container.Kernel.AddComponent(key: serviceKey, serviceType: typeof(ITerritoriesWcfService), classType: typeof(TerritoriesWcfServiceClient), lifestyle: LifestyleType.PerWebRequest);
        }
    }
}