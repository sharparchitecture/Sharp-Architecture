
namespace Northwind.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Northwind.Web.Mvc.Controllers;
    using SharpArch.Web.Mvc.Castle;

    public class MvcControllersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        
         {   container.RegisterMvcControllers(typeof(HomeController).Assembly);
        }
    }
}