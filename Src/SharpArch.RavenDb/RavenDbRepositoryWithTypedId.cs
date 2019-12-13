namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Repositories;
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
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TIdT">Primary Key type.</typeparam>
    /// <seealso cref="SharpArch.RavenDb.Contracts.Repositories.IRavenDbRepositoryWithTypedId{T, TIdT}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepositoryWithTypedId{T, TIdT}" />
    [PublicAPI]
    public class RavenDbRepositoryWithTypedId<T, TIdT> : IRavenDbRepositoryWithTypedId<T, TIdT>,
        ILinqRepositoryWithTypedId<T, TIdT>
        where T : class
    {
        /// <summary>
        ///     RavenDB Document Session.
        /// </summary>
        public IAsyncDocumentSession Session { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RavenDbRepositoryWithTypedId{T, TIdT}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepositoryWithTypedId([NotNull] IAsyncDocumentSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            TransactionManager = new TransactionManager(session);
        }

        /// <inheritdoc />
        public Task<T> FindOneAsync(TIdT id, CancellationToken cancellationToken = default)
            => GetAsync(id, cancellationToken);

        /// <inheritdoc />
        public Task<T> FindOneAsync(ILinqSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            return specification.SatisfyingElementsFrom(Session.Query<T>())
                .SingleOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     Finds all items within the repository.
        /// </summary>
        /// <returns>
        ///     All items in the repository.
        /// </returns>
        public IQueryable<T> FindAll()
            => Session.Query<T>();

        /// <summary>
        ///     Finds all items by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///     All matching items.
        /// </returns>
        public IQueryable<T> FindAll(ILinqSpecification<T> specification)
            => specification.SatisfyingElementsFrom(FindAll());

        /// <inheritdoc />
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<T>().Where(where).ToArrayAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task<T> FindOneAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<T>().Where(where).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return result
                ?? throw new InvalidOperationException("The query returned more than one result. Please refine your query.");
        }

        /// <inheritdoc />
        public async Task<T> FirstAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<T>().Where(where).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return result
                ?? throw new InvalidOperationException("The query returned no results. Please refine your query.");
        }

        /// <inheritdoc />
        public async Task<IList<T>> GetAllAsync(IEnumerable<TIdT> ids, CancellationToken cancellationToken = default)
        {
            var all = await Session.LoadAsync<T>(ids.Select(p => p.ToString()), cancellationToken).ConfigureAwait(false);
            return all.Select(kvp => kvp.Value).ToList();
        }

        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        public virtual ITransactionManager TransactionManager { get; }

        /// <inheritdoc />
        public Task<T> GetAsync(TIdT id, CancellationToken cancellationToken = default)
            => Session.LoadAsync<T>(id.ToString(), cancellationToken);

        /// <inheritdoc />
        public async Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await Session.Query<T>().ToListAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc />
        public async Task<T> SaveAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await Session.StoreAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken = default)
            => SaveAsync(entity, cancellationToken);

        /// <inheritdoc />
        public Task EvictAsync(T entity, CancellationToken cancellationToken = default)
        {
            Session.Advanced.Evict(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Session.Delete(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TIdT id, CancellationToken cancellationToken = default)
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

        /// <inheritdoc />
        IAsyncDocumentSession IRavenDbRepositoryWithTypedId<T, TIdT>.Session { get; }
    }
}
