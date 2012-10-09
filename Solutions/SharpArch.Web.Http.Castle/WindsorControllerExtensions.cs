namespace SharpArch.Web.Http.Castle
{
    using System;
    using System.Linq;
    using System.Reflection;
    using global::Castle.Core;
    using global::Castle.MicroKernel.Registration;
    using global::Castle.Windsor;
    using SharpArch.Domain;

    /// <summary>
    /// Contains Castle Windsor related HTTP controller extension methods.
    /// </summary>
    public static class WindsorControllerExtensions
    {
        /// <summary>
        /// Registers the specified HTTP controllers.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="controllerTypes">The controller types.</param>
        /// <returns>A container.</returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Type[] controllerTypes)
        {
            Check.Require(container != null);
            Check.Require(controllerTypes != null);

            foreach (Type type in controllerTypes.Where(n => ControllerExtensions.IsHttpController(n)))
            {
                container.Register(
                    Component.For(type).Named(type.FullName.ToLower()).LifeStyle.Is(LifestyleType.Transient));
            }

            return container;
        }

        /// <summary>
        /// Registers the HTTP controllers that are found in the specified assemblies.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>A container.</returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Assembly[] assemblies)
        {
            Check.Require(container != null);
            Check.Require(assemblies != null);

            foreach (Assembly assembly in assemblies)
            {
                RegisterControllers(container, assembly.GetExportedTypes());
            }

            return container;
        }
    }
}
