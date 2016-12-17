namespace SharpArch.Domain.PersistenceSupport
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines the public members of a class that implements the repository pattern for entities
    ///     of the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TId">The type of the entity ID.</typeparam>
    [PublicAPI]
    public interface IRepositoryWithTypedId<T, in TId>
    {
        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        [NotNull]
        ITransactionManager TransactionManager { get; }

        /// <summary>
        ///     Returns the entity that matches the specified ID.
        /// </summary>
        /// <remarks>
        ///     An entity or <c>null</c> if a row is not found matching the provided ID.
        /// </remarks>
        [CanBeNull]
        T Get(TId id);

        /// <summary>
        ///     Returns all of the items of a given type.
        /// </summary>
        [NotNull]
        IList<T> GetAll();

        /// <summary>
        /// For entities that have assigned Id's, you must explicitly call Save to add a new one.
        /// See http://www.hibernate.org/hib_docs/nhibernate/html_single/#mapping-declaration-id-assigned.
        /// </summary>
        /// <returns>
        /// Saved entity instance.
        /// </returns>
        [NotNull]
        T Save([NotNull] T entity);

        /// <summary>
        ///     Saves or updates the specified entity.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         For entities with automatically generated IDs, such as identity,
        ///         <see cref="SaveOrUpdate"/>  may be called when saving or updating an entity.
        ///     </para>
        ///     <para>
        ///         Updating also allows you to commit changes to a detached object.
        ///         More info may be found at:
        ///         http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        ///     </para>
        /// </remarks>
        /// <returns>
        /// Entity instance.
        /// </returns>
        [NotNull]
        T SaveOrUpdate([NotNull] T entity);


        /// <summary>
        /// Dissasociates the entity with the ORM so that changes made to it are not automatically 
        /// saved to the database.
        /// </summary>
        /// <remarks>
        /// In NHibernate this removes the entity from current session cache.
        /// More details may be found at http://www.hibernate.org/hib_docs/nhibernate/html_single/#performance-sessioncache.
        /// </remarks>
        void Evict([NotNull] T entity);


        /// <summary>
        ///     Deletes the specified entity.
        /// </summary>
        void Delete([NotNull] T entity);

        /// <summary>
        ///     Deletes the entity that matches the provided ID.
        /// </summary>
        void Delete(TId id);
    }
}