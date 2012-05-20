namespace SharpArch.Domain.PersistenceSupport
{
    using System.Collections.Generic;

    public interface IRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        /// Provides a handle to application wide DB activities such as committing any pending changes,
        /// beginning a transaction, rolling back a transaction, etc.
        /// </summary>
        IDbContext DbContext { get; }

        /// <summary>
        /// Returns null if a row is not found matching the provided Id.
        /// </summary>
        T Get(TId id);

        /// <summary>
        /// Returns all of the items of a given type.
        /// </summary>
        IList<T> GetAll();

        /// <summary>
        /// For entities with automatatically generated Ids, such as identity, SaveOrUpdate may 
        /// be called when saving or updating an entity.
        /// 
        /// Updating also allows you to commit changes to a detached object.  More info may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        T SaveOrUpdate(T entity);

        /// <summary>
        /// I'll let you guess what this does.
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Deletes the entity that matches the provided Id.
        /// </summary>
        void Delete(TId id);
    }
}