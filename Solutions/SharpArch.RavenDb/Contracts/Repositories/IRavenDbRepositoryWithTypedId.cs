namespace SharpArch.RavenDb.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Raven.Client;
    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    ///     RavenDB-speficic repository implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TIdT">The type of the identifier t.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IRepositoryWithTypedId{T, TIdT}" />
    /// Todo: Consider replacing lambda with expression tree.
    [PublicAPI]
    public interface IRavenDbRepositoryWithTypedId<T, in TIdT> : IRepositoryWithTypedId<T, TIdT>
    {
        /// <summary>
        ///     RavenDB Document Session.
        /// </summary>
        IDocumentSession Session { get; }

        /// <summary>
        ///     Finds all documents satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>Documents</returns>
        IEnumerable<T> FindAll(Func<T, bool> where);

        /// <summary>
        ///     Finds single document satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>The document</returns>
        /// <exception cref="InvalidOperationException">If more than one document found.</exception>
        T FindOne(Func<T, bool> where);

        /// <summary>
        ///     Finds the first document satisfying given criteria.
        /// </summary>
        /// <param name="where">The Criteria.</param>
        /// <returns>The document.</returns>
        /// <exception cref="InvalidOperationException">If no documents found.</exception>
        T First(Func<T, bool> where);

        /// <summary>
        ///     Loads all documents with given IDs.
        /// </summary>
        /// <param name="ids">Document IDs.</param>
        /// <returns>List of documents.</returns>
        IList<T> GetAll(IEnumerable<TIdT> ids);
    }
}
