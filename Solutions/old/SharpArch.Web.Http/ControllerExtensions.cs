namespace SharpArch.Web.Http
{
    using System;
    using System.Web.Http.Controllers;
    using JetBrains.Annotations;

    /// <summary>
    /// Contains HTTP controller related extension methods.
    /// </summary>
    [PublicAPI]
    public static class ControllerExtensions
    {
        /// <summary>
        /// Determines whether the specified type is a HTTP controller.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c>, if the type is a HTTP controller; <c>false</c>, otherwise.</returns>
        public static bool IsHttpController([CanBeNull] this Type type)
        {
            return type != null
                   && !type.IsAbstract
                   && type.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                   && typeof(IHttpController).IsAssignableFrom(type);
        }
    }
}
