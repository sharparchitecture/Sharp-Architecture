namespace SharpArch.NHibernate
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using global::NHibernate.Cfg;
    using JetBrains.Annotations;
    using global::FluentNHibernate.Infrastructure;



    /// <summary>
    ///     Provides file cache helper methods.
    /// </summary>
    static class ConfigurationFileCache
    {
        /// <summary>
        ///     Loads NHibernate configuration from file.
        /// </summary>
        /// <param name="path">Full path to file containing serialized data.</param>
        /// <returns><see cref="Configuration"/> instance, or <c>null</c> in case of deserialization error.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the path parameter is null or empty.</exception>
        public static Configuration RetrieveFromCache(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                using (FileStream file = File.Open(path, FileMode.Open))
                {
                    return Load(file);
                }
            }
            // ReSharper disable once CatchAllClause
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "ConfigurationFileCache");
                // Return null if the object can't be deserialized
                return null;
            }
        }

        /// <summary>
        ///     Serializes NHibernate configuration to a file at the given path.
        /// </summary>
        /// <param name="configuration">NHibernate configuration instance.</param>
        /// <param name="path">Full path of file to store the serialized data.</param>
        /// <exception cref="ArgumentNullException">Thrown if configuration or path parameters are null.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path" /> is a zero-length string, contains only white space, or
        ///     contains one or more invalid characters.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length.
        ///     For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than
        ///     260 characters.
        /// </exception>
        /// <exception cref="NotSupportedException"><paramref name="path" /> is in an invalid format. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     <paramref name="path" /> specified a file that is read-only.
        ///     -or- This operation is not supported on the current platform.
        ///     -or- <paramref name="path" /> specified a directory.
        ///     -or- The caller does not have the required permission.
        ///     -or specified file is a hidden file.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="SerializationException">
        ///     An error has occurred during serialization, such as if an object in the <paramref name="configuration" /> parameter is not
        ///     marked as serializable.
        /// </exception>
        public static void StoreInCache([NotNull] Configuration configuration, [NotNull] string path)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path is null or empty.", nameof(path));
            using (FileStream file = File.Open(path, FileMode.Create))
            {
                Save(file, configuration);
            }
        }

        /// <summary>
        ///     Saves object to stream in BinaryFormat.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="configuration">Configuration.</param>
        /// <exception cref="SerializationException">
        ///     An error has occurred during serialization, such as if an object in the <paramref name="configuration" /> parameter is not
        ///     marked as serializable.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        internal static void Save([NotNull] Stream stream, Configuration configuration)
        {
            CreateFormatter().Serialize(stream, configuration);
        }

        static BinaryFormatter CreateFormatter()
        {
            return new BinaryFormatter(new NetStandardSerialization.SurrogateSelector(), new StreamingContext());
        }

        /// <summary>
        ///     Load object from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><see cref="Configuration"/> instance.</returns>
        /// <exception cref="SerializationException">
        ///     The <paramref name="stream" /> supports seeking, but its length is 0.
        ///     -or-The target type is a <see cref="T:System.Decimal" />, but the value is out of range of the
        ///     <see cref="T:System.Decimal" /> type.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        internal static Configuration Load([NotNull] Stream stream)
        {
            return CreateFormatter().Deserialize(stream) as Configuration;
        }
    }
}
