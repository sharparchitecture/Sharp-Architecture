namespace SharpArch.Domain
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
#if NET462
    using System.Runtime.Serialization.Formatters.Binary;
#else
    using Newtonsoft.Json;
#endif
    using System.Security;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides file cache helper methods.
    /// </summary>
    [PublicAPI]
    public static class FileCache
    {
        /// <summary>
        ///     Deserializes a data file into an object of type {T}.
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize and return.</typeparam>
        /// <param name="path">Full path to file containing seralized data.</param>
        /// <returns>If object is successfully deserialized, the object of type {T}, otherwise null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the path parameter is null or empty.</exception>
        public static T RetrieveFromCache<T>(string path) where T : class
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                using (var file = File.Open(path, FileMode.Open))
                {
                    return Load<T>(file);
                }
            }
            // ReSharper disable once CatchAllClause
            catch(Exception ex)
            {
                Debug.WriteLine(ex, "FileCache");
                // Return null if the object can't be deserialized
                return null;
            }
        }

        /// <summary>
        ///     Serializes the given object of type {T} to a file at the given path.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="obj">Object to serialize and store in a file.</param>
        /// <param name="path">Full path of file to store the serialized data.</param>
        /// <exception cref="ArgumentNullException">Thrown if obj or path parameters are null.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. </exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
        /// <exception cref="NotSupportedException"><paramref name="path" /> is in an invalid format. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive). </exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="path" /> specified a file that is read-only.
        /// -or- This operation is not supported on the current platform.
        /// -or- <paramref name="path" /> specified a directory.
        /// -or- The caller does not have the required permission. 
        /// -or specified file is a hidden file.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="SerializationException">
        ///     An error has occurred during serialization, such as if an object in the <paramref name="obj" /> parameter is not marked as serializable. 
        /// </exception>
        public static void StoreInCache<T>([NotNull] T obj, [NotNull] string path) where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path is null or empty.", nameof(path));

            using (var file = File.Open(path, FileMode.Create))
            {
                Save(file, obj);
            }
        }

        /// <summary>
        /// Saves object to stream in BinaryFormat.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="stream">The stream.</param>
        /// <param name="obj">The object.</param>
        /// <exception cref="SerializationException">
        ///     An error has occurred during serialization, such as if an object in the <paramref name="obj" /> parameter is not marked as serializable. 
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        internal static void Save<T>(Stream stream, T obj) where T : class
        {
#if NETSTANDARD14
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonConvert.SerializeObject(obj, Formatting.None));
                sw.Flush();
            }
#else
            new BinaryFormatter().Serialize(stream, obj);
#endif
        }

        /// <summary>
        /// Load object from the stream.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>        
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="SerializationException">The <paramref name="stream" /> supports seeking, but its length is 0. 
        /// -or-The target type is a <see cref="T:System.Decimal" />, but the value is out of range of the <see cref="T:System.Decimal" /> type.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        internal static T Load<T>([NotNull] Stream stream) where T : class
        {
#if NETSTANDARD14
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
            }
#else
            return new BinaryFormatter().Deserialize(stream) as T;
#endif
        }


    }
}