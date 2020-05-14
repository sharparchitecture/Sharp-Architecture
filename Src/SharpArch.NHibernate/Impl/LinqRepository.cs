using JetBrains.Annotations;
using SharpArch.Domain.PersistenceSupport;

namespace SharpArch.NHibernate.Impl
{
    using System;
    using Domain.DomainModel;
    using MultiDb;


    /// <summary>
    ///     LINQ extensions to NHibernate repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="int" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepository{T}" />
    [PublicAPI]
    public class LinqRepository<T> : LinqRepositoryWithTypedId<T, int>, ILinqRepository<T>
        where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinqRepository{T}" /> class.
        /// </summary>
        public LinqRepository([NotNull] INHibernateSessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider databaseIdentifierProvider)
            : base(sessionRegistry, databaseIdentifierProvider)
        {
        }
    }
}
