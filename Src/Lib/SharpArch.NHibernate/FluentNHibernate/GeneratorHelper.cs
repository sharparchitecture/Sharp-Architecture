namespace SharpArch.NHibernate.FluentNHibernate
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;


    /// <summary>
    ///     An optional helper class used to view the mapping file generated from a fluent nhibernate class mapper
    /// </summary>
    /// <remarks>
    ///     This is not necessary for Fluent Nhibernate to function properly.
    /// </remarks>
    [PublicAPI]
    public class GeneratorHelper
    {
        const string GeneratorInterface = nameof(IMapGenerator);

        /// <summary>
        ///     Scans assembly for <c>IMapGenerator</c> implementers.
        /// </summary>
        /// <returns></returns>
        public static IMapGenerator[] GetMapGenerators()
        {
            var assembly = typeof(IMapGenerator).Assembly;

            return (from type in assembly.GetTypes()
                where null != type.GetInterface(GeneratorInterface)
                select Activator.CreateInstance(type)).OfType<IMapGenerator>().ToArray();
        }
    }
}
