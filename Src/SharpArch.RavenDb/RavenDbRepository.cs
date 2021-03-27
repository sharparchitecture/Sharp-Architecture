namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Repositories;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;
    using Domain.Specifications;
    using JetBrains.Annotations;
    using Raven.Client.Documents;
    using Raven.Client.Documents.Commands.Batches;
    using Raven.Client.Documents.Session;


    /// <summary>
    ///     RavenDB repository base class.
    ///     Implements repository for given entity type.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TId">Primary Key type.</typeparam>
    /// <seealso cref="IRavenDbRepository{T,TIdT}" />
    /// <seealso cref="ILinqRepository{T,TId}" />
    [PublicAPI]
    public class RavenDbRepository<TEntity, TId> : IRavenDbRepository<TEntity, TId>,
        ILinqRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     RavenDB Document Session.
        /// </summary>
        protected IAsyncDocumentSession Session { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RavenDbRepository{T,TIdT}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepository(IAsyncDocumentSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            TransactionManager = new TransactionManager(session);
        }

        /// <inheritdoc />
        public Task<TEntity> FindOneAsync(TId id, CancellationToken cancellationToken = default)
            => GetAsync(id, cancellationToken);

        /// <inheritdoc />
        public Task<TEntity> FindOneAsync(ILinqSpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            return specification.SatisfyingElementsFrom(Session.Query<TEntity>())
                .SingleOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Finds all items within the repository.
        /// </summary>
        /// <returns>
        ///     All items in the repository.
        /// </returns>
        public IQueryable<TEntity> FindAll()
            => Session.Query<TEntity>();

        /// <summary>
        ///     Finds all items by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///     All matching items.
        /// </returns>
        public IQueryable<TEntity> FindAll(ILinqSpecification<TEntity> specification)
            => specification.SatisfyingElementsFrom(FindAll());

        /// <inheritdoc />
        public async Task<TEntity[]> FindAllAsync(Expression<Func<TEntity, bool>> @where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<TEntity>().Where(where).ToArrayAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> @where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<TEntity>().Where(where).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return result
                ?? throw new InvalidOperationException("The query returned more than one result. Please refine your query.");
        }

        /// <inheritdoc />
        public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<TEntity>().Where(where).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return result
                ?? throw new InvalidOperationException("The query returned no results. Please refine your query.");
        }

        /// <inheritdoc />
        public async Task<IList<TEntity>> GetAllAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            var all = await Session.LoadAsync<TEntity>(ids.Select(p => p.ToString()), cancellationToken).ConfigureAwait(false);
            return all.Select(kvp => kvp.Value).ToList();
        }

        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        public virtual ITransactionManager TransactionManager { get; }

        /// <inheritdoc />
        public Task<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default)
            => Session.LoadAsync<TEntity>(id.ToString(), cancellationToken);

        /// <inheritdoc />
        public async Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<TEntity>().ToListAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await Session.StoreAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public Task<TEntity> SaveOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => SaveAsync(entity, cancellationToken);

        /// <inheritdoc />
        public Task EvictAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Session.Advanced.Evict(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Session.Delete(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            if (id is ValueType)
            {
                var entity = await GetAsync(id, cancellationToken).ConfigureAwait(false);
                await DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Session.Advanced.Defer(new DeleteCommandData(id.ToString(), null));
            }
        }

    }
}
