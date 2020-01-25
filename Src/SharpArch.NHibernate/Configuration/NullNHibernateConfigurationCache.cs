using System;

namespace SharpArch.NHibernate.Configuration
{
    /// <summary>
    ///     Null Object for configuration cache.
    /// </summary>
    internal class NullNHibernateConfigurationCache : INHibernateConfigurationCache
    {
        /// <summary>
        ///     Instance.
        /// </summary>
        public static readonly INHibernateConfigurationCache Null = new NullNHibernateConfigurationCache();

        private NullNHibernateConfigurationCache()
        {
        }

        /// <inheritdoc />
        public global::NHibernate.Cfg.Configuration TryLoad(DateTime localConfigurationTimestampUtc)
        {
            return null;
        }

        /// <inheritdoc />
        public void Save(global::NHibernate.Cfg.Configuration configuration, DateTime timestampUtc)
        {
        }
    }
}
