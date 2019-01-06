namespace SharpArch.NHibernate
{
    using System;
    using System.IO;
    using JetBrains.Annotations;


    /// <summary>
    ///     File-based NHibernate configuration cache.
    /// </summary>
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
