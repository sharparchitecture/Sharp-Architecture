using SharpArch.NHibernate.Impl;
using SharpArch.NHibernate.MultiDb;

namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     Prototype.
    /// </summary>
    public class TaggedNHibernateRepositoryWithTypedId<T, TId>: NHibernateRepositoryWithTypedId<T, TId> where T : class
    {
        private static INHibernateTransactionManager GetTransactionManager(
            [NotNull] ISessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider keyProvider)
        {
            if (sessionRegistry == null) throw new ArgumentNullException(nameof(sessionRegistry));
            if (keyProvider == null) throw new ArgumentNullException(nameof(keyProvider));

            return sessionRegistry.GetTransactionManager(keyProvider.GetFromType(typeof(T)));
        }

        public TaggedNHibernateRepositoryWithTypedId([NotNull] ISessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider keyProvider)
        :base(GetTransactionManager(sessionRegistry, keyProvider))
        {
        }
    }
}
