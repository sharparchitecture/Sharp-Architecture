namespace Northwind.Wcf.Web
{
    using System;
    using System.Reflection;
    using System.Web;
    using Castle.Facilities.WcfIntegration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    public class Global : HttpApplication
    {
        IWindsorContainer container;


        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            var ex = this.Server.GetLastError();
            var reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        protected void Application_Start()
        {
            this.container = this.InitializeServiceLocator();
        }

        protected void Application_End()
        {
            this.container?.Dispose();
            this.container = null;
        }

        /// <summary>
        ///   Instantiate the container and add all Controllers that derive from
        ///   WindsorController to the container.  Also associate the Controller
        ///   with the WindsorContainer ControllerFactory.
        /// </summary>
        IWindsorContainer InitializeServiceLocator()
        {
            IWindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Kernel.AddFacility<WcfFacility>();
            windsorContainer.Install(FromAssembly.This());
            return windsorContainer;
        }

    }
}