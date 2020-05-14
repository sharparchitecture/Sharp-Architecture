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
    public class AttributeBasedDatabaseIdentifierProvider : IDatabaseIdentifierProvider
    {
        static readonly Func<Type, string> _getIdDelegateCache = GetFromAttribute;
        readonly ConcurrentDictionary<Type, string> _databaseIdCache;

        /// <summary>
        ///     Creates new instance of the provider.
        /// </summary>
        public AttributeBasedDatabaseIdentifierProvider()
        {
            _databaseIdCache = new ConcurrentDictionary<Type, string>(2, 64);
        }

        /// <inheritdoc />
        public string GetFromInstance([NotNull] object anObject)
        {
            if (anObject == null) throw new ArgumentNullException(nameof(anObject));

            var type = anObject.GetType();
            return GetFromType(type);
        }

        /// <inheritdoc />
        public string GetFromType([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

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
