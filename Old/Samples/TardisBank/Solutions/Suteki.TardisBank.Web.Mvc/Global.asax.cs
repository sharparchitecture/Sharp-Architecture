namespace Suteki.TardisBank.Web.Mvc
{
    using System;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using log4net.Config;
    using SharpArch.Domain.Reflection;
    using SharpArch.Web.Mvc.Castle;
    using SharpArch.Web.Mvc.ModelBinder;


    /// <summary>
    ///     Represents the MVC Application
    /// </summary>
    /// <remarks>
    ///     For instructions on enabling IIS6 or IIS7 classic mode,
    ///     visit http://go.microsoft.com/?LinkId=9394801
    /// </remarks>
    public class MvcApplication : HttpApplication
    {
        private TypePropertyDescriptorCache injectablePropertiesCache;
        private IWindsorContainer container;
        private static readonly char Separator = ',';

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            var ex = Server.GetLastError();
            var reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        /// <summary>
        ///     Application startup.
        /// </summary>
        protected void Application_Start()
        {
            XmlConfigurator.Configure();

            AutoMapper.Mapper.Initialize(c =>
            {
                c.AddProfile<ModelMappingProfile>();
            });


            // Container
            injectablePropertiesCache = new TypePropertyDescriptorCache();
            container = InitializeContainer();

            // MVC
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();
            FilterProviders.Providers.InstallMvcFilterProvider(container, injectablePropertiesCache);
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));

            AreaRegistration.RegisterAllAreas();

            // WebAPI
            GlobalConfiguration.Configure(cfg => WebApiConfig.Register(cfg, container, injectablePropertiesCache));

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }

        /// <summary>
        ///     Instantiate the container and add all Controllers that derive from
        ///     WindsorController to the container. Also associate the Controller
        ///     with the WindsorContainer ControllerFactory.
        /// </summary>
        protected IWindsorContainer InitializeContainer()
        {
            IWindsorContainer c = new WindsorContainer();
            c.Install(FromAssembly.This());
            return c;

        }


        /// <summary>
        ///     Loads user roles from authentication ticket.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User == null) return;
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return;

            FormsIdentity id = HttpContext.Current.User.Identity as FormsIdentity;
            if (id != null)
            {
                var ticket = id.Ticket;

                // Get the stored user-data, in this case, our roles
                string userData = ticket.UserData;
                string[] roles = userData.Split(Separator);
                HttpContext.Current.User = new GenericPrincipal(id, roles);
            }
        }
    }
}
