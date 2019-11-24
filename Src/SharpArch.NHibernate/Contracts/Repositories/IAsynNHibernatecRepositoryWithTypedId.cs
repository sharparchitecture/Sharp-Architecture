namespace SharpArch.NHibernate.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     NHibernate-specific asynchronous repository extensions.
    /// </summary>
    /// <typeparam name="T">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IAsyncRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public interface IAsyncNHibernateRepositoryWithTypedId<T, in TId> : IAsyncRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        ///     Looks for zero or more instances using the properties provided.
        ///     The key of the collection should be the property name and the value should be
        ///     the value of the property to filter by.
        /// </summary>
        /// <param name="propertyValuePairs">Property name/value pairs to use as search criteria.</param>
        /// <param name="maxResults">Maximum number of entities to return, if <see langword="null" /> return all data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyValuePairs" /> is <see langword="null" /></exception>
        /// <exception cref="ArgumentException">No properties specified.</exception>
        Task<IList<T>> FindAllAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs,
            int? maxResults = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Looks for zero or more instances using the example provided.
        /// </summary>
        /// <param name="exampleInstance">Sample entity instance.</param>
        /// <param name="propertiesToExclude">Names of properties to exclude from search criteria.</param>
        /// <param name="maxResults">Maximum number of entities to return, if <see langword="null" /> return all data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<IList<T>> FindAllAsync(
            T exampleInstance, string[] propertiesToExclude,
            int? maxResults = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Looks for a single instance using the example provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        [ItemCanBeNull]
        Task<T> FindOneAsync(T exampleInstance, CancellationToken cancellationToken, params string[] propertiesToExclude);

        /// <summary>
        ///     Looks for a single instance using the property/values provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        /// <param name="propertyValuePairs">Property name/value pairs to use as search criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemCanBeNull]
        Task<T> FindOneAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns null if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="lockMode">Row Lock mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemCanBeNull]
        Task<T> GetAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<T> LoadAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="lockMode">Row Lock mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<T> LoadAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default);

        /// <summary>
        ///     For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        ///     Updating also allows you to commit changes to a detached object.  More info may be found at:
        ///     http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Entity instance.</returns>
        Task<T> UpdateAsync([NotNull] T entity, CancellationToken cancellationToken = default);
    }
}
