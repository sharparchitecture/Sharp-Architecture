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

        protected abstract byte[] GetCachedConfiguration();

        protected abstract DateTime? GetCachedTimestampUtc();

        protected abstract void SaveConfiguration(byte[] data, DateTime timestampUtc);

        protected BinaryFormatter CreateSerializer()
        {
#if NETSTANDARD
            return new BinaryFormatter(new NetStandardSerialization.SurrogateSelector(), new StreamingContext());
#else
            return new BinaryFormatter();
#endif
        }
    }


    public class NHibernateConfigurationFileCache : NHibernateConfigurationCacheBase
    {
        private string _fileName;

        /// <inheritdoc />
        public NHibernateConfigurationFileCache([NotNull] string sessionName, [NotNull] string fileName)
            : base(sessionName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            _fileName = Path.Combine(Path.GetTempPath(), fileName);
        }

        /// <inheritdoc />
        protected override byte[] GetCachedConfiguration()
        {
            return File.Exists(_fileName)
                ? File.ReadAllBytes(_fileName)
                : null;
        }

        /// <inheritdoc />
        protected override DateTime? GetCachedTimestampUtc()
        {
            return File.Exists(_fileName) ? File.GetLastWriteTimeUtc(_fileName) : (DateTime?) null;
        }

        /// <inheritdoc />
        protected override void SaveConfiguration(byte[] data, DateTime timestampUtc)
        {
            File.WriteAllBytes(_fileName, data);
            File.SetLastWriteTimeUtc(_fileName, timestampUtc);
        }
    }
}
