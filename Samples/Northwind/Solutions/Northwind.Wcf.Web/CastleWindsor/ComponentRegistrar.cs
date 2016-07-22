namespace Northwind.Wcf.Web.CastleWindsor
{
    using Castle.Facilities.WcfIntegration;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Northwind.WcfServices;

    public class ComponentRegistrar: IWindsorInstaller
    {
        public static void AddComponentsTo(IWindsorContainer container)
        {
            AddCustomRepositoriesTo(container);
            AddWcfServicesTo(container);
        }

        private static void AddCustomRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                Classes.FromAssemblyNamed("Northwind.Infrastructure").Pick().WithService.DefaultInterfaces());
        }


        private static void AddWcfServicesTo(IWindsorContainer container)
        {
            // Since the TerritoriesService.svc must be associated with a concrete class,
            // we must register the concrete implementation here as the service
            container.Register(Component.For<TerritoriesWcfService>().AsWcfService());

        }

        /// <summary>
        ///   Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer" />.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            AddCustomRepositoriesTo(container);
            AddWcfServicesTo(container);
        }
    }
}