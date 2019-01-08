namespace SharpArch.NHibernate
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using global::FluentNHibernate.Infrastructure;
    using global::NHibernate.Cfg;
    using global::NHibernate.UserTypes;
    using Infrastructure.Logging;
    using JetBrains.Annotations;


    /// <summary>
    ///     File cache implementation of INHibernateConfigurationCache.  Saves and loads a
    ///     serialized version of <see cref="Configuration" /> to a temporary file location.
    /// </summary>
    /// <remarks>
    ///     Serializing a <see cref="Configuration" /> object requires that all components
    ///     that make up the Configuration object be Serializable.  This includes any custom NHibernate
    ///     user types implementing <see cref="IUserType" />.
    /// </remarks>
    [PublicAPI]
    public abstract class NHibernateConfigurationCacheBase : INHibernateConfigurationCache
    {
        private static readonly ILog Log = LogProvider.For<NHibernateConfigurationCacheBase>();
        private readonly string _sessionName;

        /// <inheritdoc />
        protected NHibernateConfigurationCacheBase([NotNull] string sessionName)
        {
            _sessionName = sessionName ?? throw new ArgumentNullException(nameof(sessionName));
        }

        /// <inheritdoc />
        public Configuration TryLoad(DateTime localConfigurationTimestampUtc)
        {
            try
            {
                var cachedTimestampUtc = GetCachedTimestampUtc();
                if (cachedTimestampUtc.HasValue && cachedTimestampUtc.Value >= localConfigurationTimestampUtc)
                {
                    var cachedConfig = GetCachedConfiguration();
                    if (cachedConfig != null)
                    {
                        using (var ms = new MemoryStream(cachedConfig, false))
                        {
                            Log.InfoFormat("Using cached configuration for {session}", _sessionName);
                            return (Configuration) CreateSerializer().Deserialize(ms);
                        }
                    }
                }

                Log.InfoFormat("Cached configuration for {session} does not exists for outdated - {cachedTimestampUtc}",
                    _sessionName, cachedTimestampUtc);
                return null;
            }
            // ReSharper disable once CatchAllClause
            catch (Exception ex)
            {
                Log.WarnException("Error retrieving cached configuration for {session}, session configuration will be created", ex, _sessionName);
                return null;
            }
        }

        /// <inheritdoc />
        public void Save(Configuration configuration, DateTime timestampUtc)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (timestampUtc == DateTime.MinValue) throw new ArgumentException("Invalid date", nameof(timestampUtc));
            try
            {
                using (var ms = new MemoryStream(64 * 1024))
                {
                    CreateSerializer().Serialize(ms, configuration);
                    var data = ms.ToArray();
                    SaveConfiguration(data, timestampUtc);
                }
            }
            catch (Exception ex)
            {
                Log.WarnException("Error saving configuration for {session} to cache", ex, _sessionName);
            }
        }

        /// <summary>
        ///     Reads serialized configuration from cache.
        /// </summary>
        /// <returns>Configuration as byte array or <c>null</c> if configuration is not available in cache.</returns>
        /// <remarks>
        ///     Exception thrown by this method will be handled by <see cref="NHibernateConfigurationCacheBase" />.
        /// </remarks>
        [CanBeNull]
        protected abstract byte[] GetCachedConfiguration();

        /// <summary>
        ///     Returns modification of the configuration (most recent file modification date).
        ///     This timestamp is used to compare local configuration files against cached configuration.
        /// </summary>
        /// <returns>Timestamp is UTC or <c>null</c> if cached configuration is not available.</returns>
        protected abstract DateTime? GetCachedTimestampUtc();

        /// <summary>
        ///     Stores serialized configuration in cache.
        /// </summary>
        /// <param name="data">Serialized configuration.</param>
        /// <param name="timestampUtc">Timestamp of configuration.</param>
        protected abstract void SaveConfiguration(byte[] data, DateTime timestampUtc);

        /// <summary>
        ///     Creates binary serializer for NHibernate configuration.
        /// </summary>
        /// <returns>
        ///     <see cref="BinaryFormatter" />
        /// </returns>
        /// <remarks>
        ///     Override this method to provide custom serializers for NHibernate configuration classes.
        /// </remarks>
        protected virtual BinaryFormatter CreateSerializer()
        {
            return new BinaryFormatter(new NetStandardSerialization.SurrogateSelector(), new StreamingContext());
        }
    }
}
