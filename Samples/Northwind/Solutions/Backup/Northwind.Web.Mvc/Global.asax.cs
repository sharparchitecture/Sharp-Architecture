namespace Northwind.Web.Mvc
{
    using System;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using log4net.Config;

    using Microsoft.Practices.ServiceLocation;

    using Northwind.Infrastructure.NHibernateMaps;
    using Northwind.Web.Mvc.CastleWindsor;
    using Northwind.Web.Mvc.Controllers;

    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Web.Mvc;
    using SharpArch.Web.Mvc.Castle;
    using SharpArch.Web.Mvc.ModelBinder;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private WebSessionStorage webSessionStorage;

        public override void Init()
        {
            base.Init();

            // The WebSessionStorage must be created during the Init() to tie in HttpApplication events
            this.webSessionStorage = new WebSessionStorage(this);
        }

        /// <summary>
        ///   Due to issues on IIS7, the NHibernate initialization cannot reside in Init() but
        ///   must only be called once.  Consequently, we invoke a thread-safe singleton class to 
        ///   ensure it's only initialized once.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            NHibernateInitializer.Instance().InitializeNHibernateOnce(() => this.InitializeNHibernateSession());
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            var ex = this.Server.GetLastError();
            var reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }
        
        protected void Application_Start()
        {
            XmlConfigurator.Configure();

            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            // Client side validation provider
            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();

            var container = this.InitializeServiceLocator();

            AreaRegistration.RegisterAllAreas();
            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        /// <summary>
        ///   Instantiate the container and add all Controllers that derive from 
        ///   WindsorController to the container.  Also associate the Controller 
        ///   with the WindsorContainer ControllerFactory.
        /// </summary>
        protected virtual IWindsorContainer InitializeServiceLocator()
        {
            IWindsorContainer container = new WindsorContainer();
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));

            container.RegisterControllers(typeof(HomeController).Assembly);
            ComponentRegistrar.AddComponentsTo(container);

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            return container;
        }

        /// <summary>
        ///   If you need to communicate to multiple databases, you'd add a line to this method to
        ///   initialize the other database as well.
        /// </summary>
        private void InitializeNHibernateSession()
        {
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache(new[] { "Northwind.Domain" });
            NHibernateSession.Init(
                this.webSessionStorage,
                new[] { this.Server.MapPath("~/bin/Northwind.Infrastructure.dll") },
                new AutoPersistenceModelGenerator().Generate(),
                this.Server.MapPath("~/NHibernate.config"));
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}