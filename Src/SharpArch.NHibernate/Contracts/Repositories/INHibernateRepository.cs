namespace SharpArch.NHibernate.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     NHibernate-specific asynchronous repository extensions.
    /// </summary>
    /// <typeparam name="TEntity">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    /// <seealso cref="IRepository{TEntity,TId}" />
    [PublicAPI]
    public interface INHibernateRepository<TEntity, in TId> : IRepository<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : IEquatable<TId>
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
        Task<IList<TEntity>> FindAllAsync(
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
        Task<IList<TEntity>> FindAllAsync(
            TEntity exampleInstance, string[] propertiesToExclude,
            int? maxResults = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Looks for a single instance using the example provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        [ItemCanBeNull]
        Task<TEntity> FindOneAsync(TEntity exampleInstance, CancellationToken cancellationToken, params string[] propertiesToExclude);

        /// <summary>
        ///     Looks for a single instance using the property/values provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        /// <param name="propertyValuePairs">Property name/value pairs to use as search criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemCanBeNull]
        Task<TEntity> FindOneAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns null if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="lockMode">Row Lock mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemCanBeNull]
        Task<TEntity> GetAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Return the persistent instance of the given entity class with the given identifier.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemNotNull]
        Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Return the persistent instance of the given entity class with the given identifier, obtaining the specified lock
        ///     mode.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="lockMode">Row Lock mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [ItemNotNull]
        Task<TEntity> LoadAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Copy the state of the given object onto the persistent object with the same
        ///     identifier.
        ///     <para>
        ///         If there is no persistent instance currently associated with
        ///         the session, it will be loaded. Return the persistent instance.
        ///     </para>
        ///     <para>
        ///         If the given instance is unsaved, save a copy of and return it as a newly persistent
        ///         instance. The given instance does not become associated with the session.
        ///     </para>
        ///     <see cref="ISession.MergeAsync{T}(T,CancellationToken)" />
        /// </summary>
        /// <param name="entity">a detached instance with state to be copied </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An updated persistent instance.</returns>
        [ItemNotNull]
        Task<TEntity> MergeAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        ///     For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        ///     Updating also allows you to commit changes to a detached object.  More info may be found at:
        ///     http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Entity instance.</returns>
        /// <exception cref="ArgumentNullException"> <paramref name="entity" /> is <c>null</c>.</exception>
        [ItemNotNull]
        Task<TEntity> UpdateAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);
    }
}
