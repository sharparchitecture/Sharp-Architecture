namespace SharpArch.NHibernate.Contracts.Repositories
{
    using System.Collections.Generic;

    using global::NHibernate;
    using JetBrains.Annotations;
    using SharpArch.Domain;
    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public interface INHibernateRepositoryWithTypedId<T, in TId> : IRepositoryWithTypedId<T, TId>
    {
        #region Public Methods

        /// <summary>
        /// Looks for zero or more instances using the properties provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by.
        /// </summary>
        IList<T> FindAll(IDictionary<string, object> propertyValuePairs);

        /// <summary>
        /// Looks for zero or more instances using the example provided.
        /// </summary>
        IList<T> FindAll(T exampleInstance, params string[] propertiesToExclude);

        /// <summary>
        /// Looks for a single instance using the property/values provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        T FindOne(IDictionary<string, object> propertyValuePairs);

        /// <summary>
        /// Looks for a single instance using the example provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        T FindOne(T exampleInstance, params string[] propertiesToExclude);

        /// <summary>
        /// Returns null if a row is not found matching the provided Id.
        /// </summary>
        T Get(TId id, Enums.LockMode lockMode);

        /// <summary>
        /// Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        T Load(TId id);

        /// <summary>
        /// Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        T Load(TId id, Enums.LockMode lockMode);

        /// <summary>
        /// For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        /// Updating also allows you to commit changes to a detached object.  More info may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        T Update(T entity);

        #endregion
    }
}