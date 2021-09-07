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
        /// <summary>
        ///     Returns directory of assembly code base.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns>Directory path</returns>
        /// <exception cref="ArgumentNullException"><paramref name="assembly" /> is <see langword="null" /></exception>
        public static string GetAssemblyCodeBasePath(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
#if NET5_0_OR_GREATER
            var uri = new UriBuilder(assembly.Location);
#else
            var uri = new UriBuilder(assembly.CodeBase);
#endif
            var uriPath = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(uriPath)!;
        }
    }
}
