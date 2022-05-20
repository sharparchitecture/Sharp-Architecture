namespace SharpArch.NHibernate.FluentNHibernate;

using System.Runtime.CompilerServices;
using global::FluentNHibernate;
using global::FluentNHibernate.Cfg;
using global::FluentNHibernate.Mapping;
using global::FluentNHibernate.Mapping.Providers;


/// <summary>
///     Fluent NHibernate mappings extensions.
/// </summary>
[PublicAPI]
public static class FluentNHibernateExtensions
{
    /// <summary>
    ///     Adds mappings from the assembly and namespace specified by <typeparamref name="T" />.
    /// </summary>
    /// <remarks>
    ///     This method is useful when multiple databases are mapped from the same assembly
    ///     as it allows to separate mappings by namespace.
    /// </remarks>
    /// <typeparam name="T">Type to use as assembly and namespace filter.</typeparam>
    /// <param name="mappingsContainer">Mappings container</param>
    /// <returns>Container <paramref name="mappingsContainer" /></returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="mappingsContainer" /> is <c>null</c>.</exception>
    public static FluentMappingsContainer AddFromNamespaceOf<T>(this FluentMappingsContainer mappingsContainer)
    {
        if (mappingsContainer == null) throw new ArgumentNullException(nameof(mappingsContainer));
        string ns = typeof(T).Namespace!;
        var types = typeof(T).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.Namespace == ns)
            .Where(x => IsMappingOf<IMappingProvider>(x) ||
                IsMappingOf<IIndeterminateSubclassMappingProvider>(x) ||
                IsMappingOf<IExternalComponentMappingProvider>(x) ||
                IsMappingOf<IFilterDefinition>(x));

        foreach (var t in types)
        {
            mappingsContainer.Add(t);
        }

        return mappingsContainer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsMappingOf<T>(Type type)
        => !type.IsGenericType && typeof(T).IsAssignableFrom(type);
}
