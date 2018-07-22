namespace SharpArch.NHibernate.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using JetBrains.Annotations;
    using SharpArch.Domain;
    using SharpArch.Domain.PersistenceSupport;


    /// <summary>
    ///     NHibernate-specific asynchronous repository extensions.
    /// </summary>
    /// <typeparam name="T">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public interface IAsyncNHibernateRepositoryWithTypedId<T, in TId> : IAsyncRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        ///     Looks for zero or more instances using the properties provided.
        ///     The key of the collection should be the property name and the value should be
        ///     the value of the property to filter by.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="propertyValuePairs"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">No properties specified.</exception>
        Task<IList<T>> FindAllAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Looks for zero or more instances using the example provided.
        /// </summary>
        Task<IList<T>> FindAllAsync(
            T exampleInstance, string[] propertiesToExclude, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Looks for a single instance using the property/values provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        Task<T> FindOneAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Looks for a single instance using the example provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        Task<T> FindOneAsync(T exampleInstance, string[] propertiesToExclude, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Returns null if a row is not found matching the provided Id.
        /// </summary>
        Task<T> GetAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        Task<T> LoadAsync(TId id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        Task<T> LoadAsync(TId id, Enums.LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        ///     Updating also allows you to commit changes to a detached object.  More info may be found at:
        ///     http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
