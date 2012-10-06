namespace SharpArch.Web.Http.Castle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Dependencies;
    using global::Castle.Windsor;
    using SharpArch.Domain;

    /// <summary>
    /// Resolves HTTP dependencies using Castle Windsor.
    /// </summary>
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyResolver" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorDependencyResolver(IWindsorContainer container)
        {
            Check.Require(container != null);

            this.container = container;
        }

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <returns>A scope.</returns>
        public IDependencyScope BeginScope()
        {
            return this;
        }

        /// <summary>
        /// Gets the service for the specified type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>A service.</returns>
        public object GetService(Type serviceType)
        {
            if (!this.container.Kernel.HasComponent(serviceType))
            {
                return null;
            }

            return this.container.Resolve(serviceType);
        }

        /// <summary>
        /// Gets all services for the specified type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>A collection of services.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!this.container.Kernel.HasComponent(serviceType))
            {
                return new object[0];
            }

            return this.container.ResolveAll(serviceType).Cast<object>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}