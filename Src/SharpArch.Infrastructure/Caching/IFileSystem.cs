namespace SharpArch.Infrastructure.Caching
{
    using System;


    public interface IFileSystem
    {
        /// <summary>
        /// Returns directory information for path string.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetDirectoryName(string path);

        /// <summary>
        /// Determines whether specified file exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool FileExists(string path);

        /// <summary>
        /// Returns time when file or directory was last written to.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        DateTime GetLastWriteTimeUtc(string path);
    }
}