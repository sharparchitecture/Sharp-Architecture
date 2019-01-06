namespace SharpArch.Infrastructure.Caching
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using JetBrains.Annotations;


    /// <summary>
    ///     Resolves and collects file dependencies.
    ///     Use <see cref="GetLastModificationTime"/> to get latest modification timestamp of dependency list.
    /// </summary>
    public class DependencyList
    {
        private readonly List<string> _fileDependencies = new List<string>(16);

        private readonly IFileSystem _fileSystem;
        private string _basePath;
        
        public static DependencyList WithPathPrefix([NotNull] string basePath, IFileSystem fileSystem = null)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(basePath));
            return new DependencyList(fileSystem ?? new FileSystem(), basePath);
        }

        public static DependencyList WithBasePathOfAssembly([NotNull] Assembly assembly, IFileSystem fileSystem = null)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            return WithPathPrefix(GetAssemblyCodeBasePath(assembly), fileSystem);
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="fileSystem">Custom file system.</param>
        /// <param name="basePath">Base directory to use for file path resolution.</param>
        public DependencyList([NotNull] IFileSystem fileSystem, [NotNull] string basePath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("Value cannot be null or empty.", nameof(basePath));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _basePath = basePath;
        }

        /// <summary>
        ///     Add assembly containing given type.
        /// </summary>
        /// <typeparam name="TType">Type</typeparam>
        /// <returns>Self</returns>
        /// <exception cref="InvalidOperationException">Type is dynamically generated (assembly does not exists on disk).</exception>
        public DependencyList AddAssemblyOf<TType>()
        {
            var assembly = typeof(TType).Assembly;
            return AddAssembly(assembly);
        }

        /// <summary>
        /// Adds assembly to dependency list.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <exception cref="InvalidOperationException">Assembly is dynamically generated.</exception>
        /// <returns>Self</returns>
        public DependencyList AddAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic)
                throw new InvalidOperationException($"Cannot get location for dynamically created assembly '{assembly.GetName().Name}'");
            _fileDependencies.Add(assembly.Location);
            return this;
        }

        /// <summary>
        /// Adds multiple assemblies to dependency list.
        /// </summary>
        /// <param name="assemblies">List of assemblies.</param>
        /// <exception cref="InvalidOperationException">Assembly list contains dynamically generated assembly.</exception>
        /// <returns>Self</returns>
        public DependencyList AddAssemblies([NotNull] IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            foreach (var assembly in assemblies) AddAssembly(assembly);

            return this;
        }

        /// <summary>
        ///     Adds file to dependency list.
        /// </summary>
        /// <param name="fileName">File path, if relative, base path will be used as root.</param>
        /// <returns>Self</returns>
        /// <exception cref="ArgumentException"><paramref name="fileName" /> is <c>null</c> or whitespace.</exception>
        public DependencyList AddFile([NotNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

            _fileDependencies.Add(FindFile(fileName));
            return this;
        }

        /// <summary>
        ///     Adds files to dependency list. <see cref="AddFile" /> is performed for each dependency.
        /// </summary>
        /// <param name="files">List of files.</param>
        /// <returns>Self</returns>
        public DependencyList AddFiles([NotNull] IEnumerable<string> files)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            foreach (var fileName in files) AddFile(fileName);

            return this;
        }

        private string GetCodeBasePath()
        {
            return _basePath ?? (_basePath = GetAssemblyCodeBasePath(Assembly.GetExecutingAssembly()));
        }

        /// <summary>
        ///     Returns directory of assembly code base.
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns>Directory path</returns>
        /// <exception cref="ArgumentNullException"><paramref name="assembly" /> is <see langword="null" /></exception>
        [NotNull]
        public static string GetAssemblyCodeBasePath([NotNull] Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            var uri = new UriBuilder(assembly.CodeBase);
            var uriPath = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(uriPath);
        }

        /// <summary>
        ///     Tests if the file or assembly name exists either in the application's bin folder
        ///     or elsewhere.
        /// </summary>
        /// <param name="path">Path or file name to test for existence.</param>
        /// <returns>Full path of the file.</returns>
        /// <remarks>
        ///     If the path parameter does not end with ".dll" it is appended and
        ///     tested if the dll file exists.
        /// </remarks>
        /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
        private string FindFile(string path)
        {
            var codeLocation = GetCodeBasePath();

            // add base path to relative path
            var codePath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(codeLocation, path);
            if (_fileSystem.FileExists(codePath)) return codePath;

            // try with .dll extension added
            var dllPath = path.IndexOf(".dll", StringComparison.InvariantCultureIgnoreCase) == -1 
                ? path + ".dll" 
                : path;
            if (_fileSystem.FileExists(dllPath)) return dllPath;

            // try with .dll extension and base path
            var codeDllPath = Path.Combine(codeLocation, dllPath);
            if (_fileSystem.FileExists(codeDllPath)) return codeDllPath;

            throw new FileNotFoundException("Unable to find file.", path);
        }

        /// <summary>
        ///     Return list of dependencies.
        /// </summary>
        /// <returns></returns>
        public string[] Build()
        {
            return _fileDependencies.Distinct().ToArray();
        }

        /// <summary>
        ///     Gets latest modification time of all dependencies.
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastModificationTime()
        {
            return _fileDependencies.Count == 0
                ? (DateTime?) null
                : _fileDependencies.Distinct().Select(fn => _fileSystem.GetLastWriteTimeUtc(fn)).Max();
        }
    }
}
