using System;
using JetBrains.Annotations;
using NHibernate.Cfg;

namespace SharpArch.NHibernate.Configuration
{
    /// <summary>
    ///     Interface for providing caching capability for an <see cref="Configuration" /> object.
    /// </summary>
    [PublicAPI]
    public interface INHibernateConfigurationCache
    {
        /// <summary>
        ///     Load the <see cref="Configuration" /> object from a cache.
        /// </summary>
        /// <returns>
        ///     If an up to date cached object is available, a <see cref="Configuration" /> object, otherwise null.
        /// </returns>
        [CanBeNull]
        global::NHibernate.Cfg.Configuration TryLoad(DateTime localConfigurationTimestampUtc);

        /// <summary>
        ///     Save the <see cref="Configuration" /> object to a cache.
        /// </summary>
        /// <param name="configuration">Configuration object to save.</param>
        /// <param name="timestampUtc">Configuration modification date.</param>
        void Save([NotNull] global::NHibernate.Cfg.Configuration configuration, DateTime timestampUtc);
    }
}
