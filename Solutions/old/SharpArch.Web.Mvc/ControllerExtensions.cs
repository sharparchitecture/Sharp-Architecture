// ----------------------------------------------------------
// Taken from MvcContrib to reduce number of dependencies. Was only method in MvcContrib being used in project.
// ----------------------------------------------------------

namespace SharpArch.Web.Mvc
{
    using System;
    using System.Web.Mvc;
    using JetBrains.Annotations;

    /// <summary>
    /// ASP.NET MVC controller extensions.
    /// </summary>
    [PublicAPI]
    public class ControllerExtensions
    {
        /// <summary>
        /// Determines whether the specified type is a controller
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type is a controller, otherwise false</returns>
        public static bool IsController([CanBeNull] Type type)
        {
            return type != null
                   && type.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                   && !type.IsAbstract
                   && typeof(IController).IsAssignableFrom(type);
        }
    }
}