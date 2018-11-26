namespace SharpArch.NHibernate
{
    using System;
    using global::NHibernate.Cfg;


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
        public Configuration TryLoad(DateTime localConfigurationTimestampUtc)
        {
            return null;
        }

        /// <inheritdoc />
        public void Save(Configuration configuration, DateTime timestampUtc)
        {
        }
    }
}
