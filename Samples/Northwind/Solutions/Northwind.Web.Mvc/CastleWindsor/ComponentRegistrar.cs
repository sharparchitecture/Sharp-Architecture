namespace Northwind.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Northwind.WcfServices;
    using Castle.Facilities.WcfIntegration;


    public class ComponentRegistrar: IWindsorInstaller
    {


        private static void AddWcfServiceFactoriesTo(IWindsorContainer container)
        {
            //container.AddFacility("factories", new FactorySupportFacility());
            //container.AddComponent("standard.interceptor", typeof(StandardInterceptor));

            container.Register(Component.For<ITerritoriesWcfService>().AsWcfClient());
        }

        /// <summary>
        ///   Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer" />.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            AddWcfServiceFactoriesTo(container);
        }
    }
}