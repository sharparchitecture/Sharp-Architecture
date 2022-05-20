namespace SharpArch.Domain.Reflection;

using System.Collections.Concurrent;


/// <summary>
///     Property descriptors cache.
/// </summary>
/// <remarks>Implementation is thread-safe.</remarks>
[PublicAPI]
public class TypePropertyDescriptorCache : ITypePropertyDescriptorCache
{
    readonly ConcurrentDictionary<Type, TypePropertyDescriptor> _cache = new();

    /// <summary>
    ///     Find cached property descriptor.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    ///     <see cref="TypePropertyDescriptor" /> or <c>null</c> if does not exists.
    /// </returns>
    [MustUseReturnValue]
    public TypePropertyDescriptor? Find(Type type)
    {
        _cache.TryGetValue(type, out TypePropertyDescriptor? result);
        return result;
    }

    /// <summary>
    ///     Get existing property descriptor or create and cache it.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="factory">The factory to create descriptor.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="type" /> or <paramref name="factory" /> is
    ///     <see langword="null" />.
    /// </exception>
    public TypePropertyDescriptor GetOrAdd(Type type, Func<Type, TypePropertyDescriptor> factory)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        return _cache.GetOrAdd(type, factory);
    }

    /// <summary>
    ///     Clears the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    ///     Returns number of entries in the cache.
    /// </summary>
    public int Count => _cache.Count;
}
