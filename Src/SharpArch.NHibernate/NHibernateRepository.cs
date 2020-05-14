using JetBrains.Annotations;

namespace SharpArch.NHibernate.Impl
{
    using System;
    using Domain.DomainModel;


    /// <summary>
    ///     NHibernate repository implementation.
    /// </summary>
    /// <remarks>
    /// This implementation should be used in simplified, single-database model.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    [PublicAPI]
    public class NHibernateRepository<TEntity, TId> : NHibernateRepositoryBase<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <inheritdoc />
        public NHibernateRepository([NotNull] INHibernateTransactionManager transactionManager) : base(transactionManager)
        {
        }
    }
}
