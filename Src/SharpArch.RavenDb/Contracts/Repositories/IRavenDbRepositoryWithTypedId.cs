namespace SharpArch.RavenDb.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Raven.Client.Documents.Session;


    /// <summary>
    ///     RavenDB-specific repository implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TIdT">The type of the identifier t.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IAsyncRepositoryWithTypedId{T, TIdT}" />
    [PublicAPI]
    public interface IRavenDbRepositoryWithTypedId<T, in TIdT> : IAsyncRepositoryWithTypedId<T, TIdT>
        where T : class
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
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Finds single document satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The document</returns>
        /// <exception cref="InvalidOperationException">If more than one document found.</exception>
        Task<T> FindOneAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Finds the first document satisfying given criteria.
        /// </summary>
        /// <param name="where">The Criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The document.</returns>
        /// <exception cref="InvalidOperationException">If no documents found.</exception>
        Task<T> FirstAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Loads all documents with given IDs.
        /// </summary>
        /// <param name="ids">Document IDs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of documents.</returns>
        Task<IList<T>> GetAllAsync(IEnumerable<TIdT> ids, CancellationToken cancellationToken = default);
    }
}
