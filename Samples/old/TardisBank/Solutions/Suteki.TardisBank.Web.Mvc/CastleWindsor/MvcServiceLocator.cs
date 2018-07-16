namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Microsoft.Practices.ServiceLocation;

    public class MvcServiceLocator: ServiceLocatorImplBase
    {
        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of resolving
        ///             the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param><param name="key">Name of registered service you want. May be null.</param>
        /// <returns>
        /// The requested service instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">Named instances are not supported</exception>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (!string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Named instances are not supported");

            return DependencyResolver.Current.GetService(serviceType);
        }

        /// <summary>
        /// When implemented by inheriting classes, this method will do the actual work of
        ///             resolving all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>
        /// Sequence of service instance objects.
        /// </returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return DependencyResolver.Current.GetServices(serviceType);
        }
    }
}