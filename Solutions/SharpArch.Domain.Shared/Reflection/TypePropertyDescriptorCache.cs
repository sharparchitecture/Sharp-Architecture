namespace SharpArch.Domain.Reflection
{
    using System;
    using System.Collections.Concurrent;
    using JetBrains.Annotations;

    /// <summary>
    ///     Property descriptors cache.
    /// </summary>
    /// <remarks>Implementation is thread-safe.</remarks>
    [PublicAPI]
    public class TypePropertyDescriptorCache : ITypePropertyDescriptorCache
    {
        private readonly ConcurrentDictionary<Type, TypePropertyDescriptor> cache =
            new ConcurrentDictionary<Type, TypePropertyDescriptor>();

        /// <summary>
        ///     Find cached property descriptor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <see cref="TypePropertyDescriptor" /> or <c>null</c> if does not exists.
        /// </returns>
        [MustUseReturnValue]
        public TypePropertyDescriptor Find(Type type)
        {
            TypePropertyDescriptor result;
            this.cache.TryGetValue(type, out result);
            return result;
        }

        /// <summary>
        ///     Get existing property descriptor or create and cache it.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory to create descriptor.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="factory"/> is <see langword="null" />.</exception>
        public TypePropertyDescriptor GetOrAdd(Type type, Func<Type, TypePropertyDescriptor> factory)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            return this.cache.GetOrAdd(type, factory);
        }

        /// <summary>
        ///     Clears the cache.
        /// </summary>
        public void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        ///     Returns number of entries in the cache.
        /// </summary>
        public int Count => this.cache.Count;
    }
}
