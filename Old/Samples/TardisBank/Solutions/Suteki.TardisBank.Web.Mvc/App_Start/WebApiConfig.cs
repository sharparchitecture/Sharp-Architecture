using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Suteki.TardisBank.Web.Mvc
{
    using Castle.Windsor;
    using SharpArch.Domain.Reflection;
    using SharpArch.Web.Http.Castle;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config, IWindsorContainer container, TypePropertyDescriptorCache propertyDescriptorCache)
        {
            // Web API configuration and services
            config.UseWindsor(container, propertyDescriptorCache);


            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
