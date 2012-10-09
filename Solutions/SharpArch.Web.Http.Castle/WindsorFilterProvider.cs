namespace SharpArch.Web.Http.Castle
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using global::Castle.Windsor;
    using SharpArch.Domain;

    /// <summary>
    /// A Castle Windsor filter provider that supports property injection.
    /// </summary>
    public class WindsorFilterProvider : IFilterProvider
    {
        /// <summary>
        /// The container
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorFilterProvider" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public WindsorFilterProvider(IWindsorContainer container)
        {
            Check.Require(container != null);

            this.container = container;
        }

        /// <summary>
        /// Returns an enumeration of filters.
        /// </summary>
        /// <param name="configuration">The HTTP configuration.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>An enumeration of filters.</returns>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            Check.Require(configuration != null);
            Check.Require(actionDescriptor != null);

            IEnumerable<FilterInfo> globalFilters =
                configuration.Filters.Select(n => new FilterInfo(n.Instance, FilterScope.Global));

            IEnumerable<FilterInfo> controllerFilters = actionDescriptor
                .ControllerDescriptor
                .GetFilters()
                .Select(n => new FilterInfo(n, FilterScope.Controller));

            IEnumerable<FilterInfo> actionFilters = actionDescriptor
                .GetFilters()
                .Select(n => new FilterInfo(n, FilterScope.Action));

            IEnumerable<FilterInfo> filters = globalFilters.Concat(controllerFilters.Concat(actionFilters));
            this.container.Kernel.ResolveProperties(filters.Select(n => n.Instance));

            return filters;
        }
    }
}
