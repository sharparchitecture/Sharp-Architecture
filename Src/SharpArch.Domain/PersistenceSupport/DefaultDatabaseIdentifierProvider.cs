namespace SharpArch.Domain.PersistenceSupport
{
    using System;
    using System.Collections.Concurrent;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;


    /// <summary>
    ///     Implementation of <see cref="IDatabaseIdentifierProvider" /> that uses
    ///     the <see cref="UseDatabaseAttribute" /> to determine the database identifier.
    /// </summary>
    public class DefaultDatabaseIdentifierProvider : IDatabaseIdentifierProvider
    {
        static readonly Func<Type, string> _getIdDelegateCache = GetFromAttribute;
        readonly ConcurrentDictionary<Type, string> _databaseIdCache;
        readonly bool _useMultipleDatabases;

        /// <summary>
        ///     Creates new instance of the provider.
        /// </summary>
        /// <param name="useMultipleDatabases">
        ///     Value indicating that application is configured to use multiple databases.
        ///     If <c>true</c>, it will perform <see cref="UseDatabaseAttribute" /> lookup;
        ///     setting it <c>false</c> allows to speedup process a little by skipping lookups and always return
        ///     <see cref="DatabaseIdentifier.Default" />.
        /// </param>
        public DefaultDatabaseIdentifierProvider(bool useMultipleDatabases)
        {
            _useMultipleDatabases = useMultipleDatabases;
            _databaseIdCache = useMultipleDatabases
                ? new ConcurrentDictionary<Type, string>(4, 64)
                : null;
        }

        /// <inheritdoc />
        public string GetFromInstance([NotNull] object anObject)
        {
            if (anObject == null) throw new ArgumentNullException(nameof(anObject));
            if (!_useMultipleDatabases) return DatabaseIdentifier.Default;

            var type = anObject.GetType();
            return GetFromType(type);
        }

        /// <inheritdoc />
        public string GetFromType([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!_useMultipleDatabases) return DatabaseIdentifier.Default;

            var databaseId = _databaseIdCache.GetOrAdd(type, _getIdDelegateCache);
            return databaseId;
        }

        static string GetFromAttribute(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(UseDatabaseAttribute), true);
            if (attrs.Length > 0)
            {
                var databaseIdentifierAttribute = (UseDatabaseAttribute) attrs[0];
                return databaseIdentifierAttribute.DatabaseIdentifier;
            }

            return DatabaseIdentifier.Default;
        }
    }
}
