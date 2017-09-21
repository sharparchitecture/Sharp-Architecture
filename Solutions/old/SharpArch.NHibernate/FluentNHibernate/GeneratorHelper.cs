namespace SharpArch.NHibernate.FluentNHibernate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
        private const string GeneratorInterface = "IMapGenerator";

        public static IList<IMapGenerator> GetMapGenerators()
        {
            var assembly = Assembly.GetAssembly(typeof(IMapGenerator));

            return (from type in assembly.GetTypes()
                    where null != type.GetInterface(GeneratorInterface)
                    select Activator.CreateInstance(type)).OfType<IMapGenerator>().ToList();
        }
    }
}