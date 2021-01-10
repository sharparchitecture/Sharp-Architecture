namespace SharpArch.RavenDb.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Raven.Client.Documents.Session;


    /// <summary>
    ///     RavenDB-specific repository implementation.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIdT">The type of the identifier t.</typeparam>
    /// <seealso cref="IRepository{TEntity,TId}" />
    [PublicAPI]
    public interface IRavenDbRepository<TEntity, in TIdT> : IRepository<TEntity, TIdT>
        where TEntity : class, IEntity<TIdT>
        where TIdT : IEquatable<TIdT>
    {
        /// <summary>
        ///     RavenDB Document Session.
        /// </summary>
        IAsyncDocumentSession Session { get; }

        /// <summary>
        ///     Finds all documents satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Documents</returns>
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Finds single document satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The document</returns>
        /// <exception cref="InvalidOperationException">If more than one document found.</exception>
        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Finds the first document satisfying given criteria.
        /// </summary>
        /// <param name="where">The Criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The document.</returns>
        /// <exception cref="InvalidOperationException">If no documents found.</exception>
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Loads all documents with given IDs.
        /// </summary>
        /// <param name="ids">Document IDs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of documents.</returns>
        Task<IList<TEntity>> GetAllAsync(IEnumerable<TIdT> ids, CancellationToken cancellationToken = default);
    }
}
