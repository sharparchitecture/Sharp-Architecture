namespace SharpArch.Web.Http.Castle
{
    using System;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Contains HTTP controller related extension methods.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Determines whether the specified type is a HTTP controller.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c>, if the type is a HTTP controller; <c>false</c>, otherwise.</returns>
        public static bool IsHttpController(Type type)
        {
            return type != null
                   && type.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                   && !type.IsAbstract
                   && typeof(IHttpController).IsAssignableFrom(type);
        }
    }
}
