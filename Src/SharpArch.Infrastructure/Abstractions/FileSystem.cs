namespace SharpArch.Infrastructure.Abstractions
{
    using System;
    using System.IO;


    /// <summary>
    ///     File system abstraction.
    /// </summary>
    public class FileSystem : IFileSystem
    {
        /// <summary>
        ///     Returns directory information for path string.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        ///     Determines whether specified file exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        ///     Returns time when file or directory was last written to.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }
    }
}
