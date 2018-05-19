namespace SharpArch.NHibernate
{
    using System.Collections.Generic;
    using global::NHibernate.Cfg;

    /// <summary>
    /// Null Object for configuration cache.
    /// </summary>
    class NullNHibernateConfigurationCache : INHibernateConfigurationCache
    {
        private NullNHibernateConfigurationCache()
        {
        }

        /// <summary>
        /// Instance.
        /// </summary>
        public static readonly INHibernateConfigurationCache Null = new NullNHibernateConfigurationCache();

        public Configuration LoadConfiguration(string configKey, string configPath, IEnumerable<string> mappingAssemblies)
        {
            return null;
        }

        public void SaveConfiguration(string configKey, Configuration config)
        {
            
        }
    }
}