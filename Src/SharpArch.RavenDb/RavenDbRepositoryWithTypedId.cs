namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Repositories;
    using Domain.PersistenceSupport;
    using Domain.Specifications;
    using JetBrains.Annotations;
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

        /// <inheritdoc />
        public Task<IEnumerable<T>> FindAllAsync(Func<T, bool> @where, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> FindOneAsync(Func<T, bool> @where, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> FirstAsync(Func<T, bool> @where, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IList<T>> GetAllAsync(IEnumerable<TIdT> ids, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RavenDbRepositoryWithTypedId{T, TIdT}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepositoryWithTypedId([NotNull] IAsyncDocumentSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            TransactionManager = new TransactionManager(session);
        }

        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        public virtual ITransactionManager TransactionManager { get; }

        /// <inheritdoc />
        public Task<T> GetAsync(TIdT id, CancellationToken cancellationToken = default)
        {
            return Session.LoadAsync<T>(id.ToString(), cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> SaveAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task EvictAsync(T entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
        public Task<T> FindOneAsync(TIdT id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> FindOneAsync(ILinqSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IQueryable<T> FindAll(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IQueryable<T> FindAll(ILinqSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IAsyncDocumentSession IRavenDbRepositoryWithTypedId<T, TIdT>.Session { get; }

        /// <summary>
        ///     Finds all documents satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>
        ///     Documents
        /// </returns>
        public IEnumerable<T> FindAll(Func<T, bool> where)
        {
            return Session.Query<T>().
        }

        /// <summary>
        ///     Finds single document satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>
        ///     The document
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The query returned more than one result. Please refine your query.</exception>
        public T FindOne(Func<T, bool> where)
        {
            IEnumerable<T> foundList = FindAll(where);

            try
            {
                return foundList.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    "The query returned more than one result. Please refine your query.", ex);
            }
        }

        /// <summary>
        ///     Finds the first document satisfying fiven criteria.
        /// </summary>
        /// <param name="where">The Criteria.</param>
        /// <returns>
        ///     The document.
        /// </returns>
        public T First(Func<T, bool> where)
        {
            return FindAll(where).First(where);
        }

        /// <summary>
        ///     Finds a document by ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>
        ///     The matching document.
        /// </returns>
        public T FindOne(TIdT id)
        {
            return Get(id);
        }

        /// <summary>
        ///     Finds an item by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///     The matching item.
        /// </returns>
        public T FindOne(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(FindAll()).SingleOrDefault();
        }

        /// <summary>
        ///     Finds all items within the repository.
        /// </summary>
        /// <returns>
        ///     All items in the repository.
        /// </returns>
        public IQueryable<T> FindAll()
        {
            return Session.Query<T>();
        }

        /// <summary>
        ///     Finds all items by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        ///     All matching items.
        /// </returns>
        public IQueryable<T> FindAll(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(FindAll());
        }

        /// <summary>
        ///     Stores document in session.
        /// </summary>
        /// <param name="entity">The document.</param>
        /// <returns>Stored document.</returns>
        public T Save(T entity)
        {
            return SaveOrUpdate(entity);
        }

        /// <summary>
        ///     Dissasociates the entity with the ORM so that changes made to it are not automatically
        ///     saved to the database.
        /// </summary>
        /// <param name="entity"></param>
        public void Evict(T entity)
        {
            Session.Advanced.Evict(entity);
        }

        /// <summary>
        ///     Returns all of the items of a given type.
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAll()
        {
            return FindAll().ToList();
        }

        /// <summary>
        ///     Loads all documents with given IDs.
        /// </summary>
        /// <param name="ids">Document IDs.</param>
        /// <returns>
        ///     List of documents.
        /// </returns>
        public async Task<IList<T>> GetAll(IEnumerable<TIdT> ids, CancellationToken cancellationToken)
        {
            var all = await Session.LoadAsync<T>(ids.Select(p => p.ToString()), cancellationToken).ConfigureAwait(false);
            return all.Select(kvp => kvp.Value).ToList();
        }

        /// <summary>
        ///     Saves or updates the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         For entities with automatically generated IDs, such as identity,
        ///         <see cref="M:SharpArch.Domain.PersistenceSupport.IRepositoryWithTypedId`2.SaveOrUpdate(`0)" />  may be called
        ///         when saving or updating an entity.
        ///     </para>
        ///     <para>
        ///         Updating also allows you to commit changes to a detached object.
        ///         More info may be found at:
        ///         http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        ///     </para>
        /// </remarks>
        public async Task<T> SaveOrUpdate(T entity, CancellationToken cancellationToken)
        {
            await Session.StoreAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }
    }
}
