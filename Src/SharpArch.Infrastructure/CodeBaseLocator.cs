namespace SharpArch.Infrastructure
{
    using System;
    using System.IO;
    using System.Reflection;
    using JetBrains.Annotations;


    /// <summary>
    ///     Resolves assembly code base directory.
    /// </summary>
    [PublicAPI]
    public class CodeBaseLocator
    {
        internal static string GetAssemblyPath(Assembly assembly)
        {
            return
#if NET5_0_OR_GREATER
                assembly.Location
#else
                assembly.CodeBase
#endif
                ;
        }

        /// <summary>
        ///     Returns directory of assembly code base.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns>Directory path</returns>
        /// <exception cref="ArgumentNullException"><paramref name="assembly" /> is <see langword="null" /></exception>
        public static string GetAssemblyCodeBasePath(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var uri = new UriBuilder(GetAssemblyPath(assembly));
            var uriPath = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(uriPath)!;
        }
    }
}
