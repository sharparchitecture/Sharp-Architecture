namespace SharpArch.Infrastructure.Abstractions
{
    using System;


    /// <summary>
    ///     File system abstraction.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        ///     Determines whether specified file exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool FileExists(string path);

        /// <summary>
        ///     Returns time when file or directory was last written to.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        DateTime GetLastWriteTimeUtc(string path);
    }
}
