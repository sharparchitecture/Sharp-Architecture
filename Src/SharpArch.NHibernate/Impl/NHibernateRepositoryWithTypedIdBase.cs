using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Criterion;
using SharpArch.Domain;
using SharpArch.Domain.PersistenceSupport;
using SharpArch.NHibernate.Contracts.Repositories;

namespace SharpArch.NHibernate.Impl
{
    /// <summary>
    /// Base NHibernate repository implementation.
    /// </summary>
    /// <remarks>
    /// Keep constructor protected to enable support of single-database (<see cref="NHibernateRepositoryWithTypedId{T,TId}"/>)
    /// and multiple-database
    /// </remarks>
    /// <typeparam name="TEntity">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    [PublicAPI]
    public class NHibernateRepositoryWithTypedIdBase<TEntity, TId> : IAsyncNHibernateRepositoryWithTypedId<TEntity, TId>
        where TEntity : class
    {
        /// <summary>
        ///     Returns NHibernate session.
        /// </summary>
        protected ISession Session => TransactionManager.Session;

        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        protected INHibernateTransactionManager TransactionManager { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NHibernateRepositoryBase{TEntity,TId}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="transactionManager"/> is <c>null</c>.</exception>
        protected NHibernateRepositoryWithTypedIdBase(
            [NotNull] INHibernateTransactionManager transactionManager)
        {
            TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        }

        /// <inheritdoc />
        public Task<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default)
            => Session.GetAsync<TEntity>(id, cancellationToken);

        /// <inheritdoc />
        public Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            ICriteria criteria = Session.CreateCriteria(typeof(TEntity));
            return criteria.ListAsync<TEntity>(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public async Task<TEntity> SaveOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public Task EvictAsync(TEntity entity, CancellationToken cancellationToken = default)
            => Session.EvictAsync(entity, cancellationToken);

        /// <inheritdoc />
        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
            => Session.DeleteAsync(entity, cancellationToken);

        /// <inheritdoc />
        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            var entity = await GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity != null) await DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        ITransactionManager IAsyncRepositoryWithTypedId<TEntity, TId>.TransactionManager => TransactionManager;

        /// <inheritdoc />
        public virtual Task<IList<TEntity>> FindAllAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs,
            int? maxResults = null,
            CancellationToken cancellationToken = default)
        {
            if (propertyValuePairs == null) throw new ArgumentNullException(nameof(propertyValuePairs));
            if (propertyValuePairs.Count == 0)
                throw new ArgumentException("No properties specified. Please specify at least one property/value pair.",
                    nameof(propertyValuePairs));

            ICriteria criteria = Session.CreateCriteria(typeof(TEntity));
            foreach (string key in propertyValuePairs.Keys)
            {
                criteria.Add(propertyValuePairs[key] != null
                    ? Restrictions.Eq(key, propertyValuePairs[key])
                    : Restrictions.IsNull(key));
            }

            if (maxResults.HasValue) criteria.SetMaxResults(maxResults.Value);
            return criteria.ListAsync<TEntity>(cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TEntity>> FindAllAsync(
            TEntity exampleInstance, string[] propertiesToExclude, int? maxResults = null, CancellationToken cancellationToken = default)
        {
            ICriteria criteria = Session.CreateCriteria(typeof(TEntity));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude) example.ExcludeProperty(propertyToExclude);

            criteria.Add(example);

            if (maxResults.HasValue) criteria.SetMaxResults(maxResults.Value);

            return criteria.ListAsync<TEntity>(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> FindOneAsync(TEntity exampleInstance, CancellationToken ct, params string[] propertiesToExclude)
        {
            IList<TEntity> foundList = await FindAllAsync(exampleInstance, propertiesToExclude, 2, ct).ConfigureAwait(false);
            if (foundList.Count > 1) throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1 ? foundList[0] : default;
        }

        /// <inheritdoc />
        public async Task<TEntity> FindOneAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs,
            CancellationToken cancellationToken = default)
        {
            var foundList = await FindAllAsync(propertyValuePairs, 2, cancellationToken).ConfigureAwait(false);
            if (foundList.Count > 1) throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1 ? foundList[0] : default;
        }

        /// <inheritdoc />
        public virtual Task<TEntity> GetAsync(TId id, Enums.LockMode lockMode, CancellationToken ct)
            => Session.GetAsync<TEntity>(id, ConvertFrom(lockMode), ct);

        /// <inheritdoc />
        public virtual Task<TEntity> LoadAsync(TId id, CancellationToken ct)
            => Session.LoadAsync<TEntity>(id, ct);

        /// <inheritdoc />
        public virtual Task<TEntity> LoadAsync(TId id, Enums.LockMode lockMode, CancellationToken ct)
            => Session.LoadAsync<TEntity>(id, ConvertFrom(lockMode), ct);

        /// <inheritdoc />
        public Task<T> MergeAsync(T entity, CancellationToken cancellationToken = default)
            => Session.MergeAsync(entity, cancellationToken);

        /// <inheritdoc />
        public virtual async Task<T> UpdateAsync(T entity, CancellationToken ct)
        {
            await Session.UpdateAsync(entity, ct).ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        ///     Translates a domain layer lock mode into an NHibernate lock mode via reflection.  This is
        ///     provided to facilitate developing the domain layer without a direct dependency on the
        ///     NHibernate assembly.
        /// </summary>
        static LockMode ConvertFrom(Enums.LockMode lockMode)
        {
            switch (lockMode)
            {
                case Enums.LockMode.None:
                    return LockMode.None;
                case Enums.LockMode.Read:
                    return LockMode.Read;
                case Enums.LockMode.Upgrade:
                    return LockMode.Upgrade;
                case Enums.LockMode.UpgradeNoWait:
                    return LockMode.UpgradeNoWait;
                case Enums.LockMode.Write:
                    return LockMode.Write;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lockMode), lockMode,
                        "The provided lock mode , '" + lockMode +
                        "', could not be translated into an NHibernate.LockMode. " +
                        "This is probably because NHibernate was updated and now has different lock modes which are out of synch " +
                        "with the lock modes maintained in the domain layer.");
            }
        }
    }
}
