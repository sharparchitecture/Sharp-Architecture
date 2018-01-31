namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Raven.Abstractions.Commands;
    using Raven.Client;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Domain.Specifications;
    using SharpArch.RavenDb.Contracts.Repositories;

    /// <summary>
    /// RavenDB repository base class.
    /// Implements repository for given entity type.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TIdT">Primary Key type.</typeparam>
    /// <seealso cref="SharpArch.RavenDb.Contracts.Repositories.IRavenDbRepositoryWithTypedId{T, TIdT}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepositoryWithTypedId{T, TIdT}" />
    [PublicAPI]
    public class RavenDbRepositoryWithTypedId<T, TIdT> : IRavenDbRepositoryWithTypedId<T, TIdT>,
        ILinqRepositoryWithTypedId<T, TIdT>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDbRepositoryWithTypedId{T, TIdT}"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepositoryWithTypedId(IDocumentSession session)
        {
            this.Session = session;
            this.TransactionManager = new TransactionManager(session);
        }

        /// <summary>
        /// Finds a document by ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>
        /// The matching document.
        /// </returns>
        public T FindOne(TIdT id)
        {
            return this.Get(id);
        }

        /// <summary>
        /// Finds an item by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        /// The matching item.
        /// </returns>
        public T FindOne(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(this.FindAll()).SingleOrDefault();
        }

        /// <summary>
        /// Finds all items within the repository.
        /// </summary>
        /// <returns>
        /// All items in the repository.
        /// </returns>
        public IQueryable<T> FindAll()
        {
            return this.Session.Query<T>();
        }

        /// <summary>
        /// Finds all items by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        /// All matching items.
        /// </returns>
        public IQueryable<T> FindAll(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(this.FindAll());
        }

        /// <summary>
        /// RavenDB Document Session.
        /// </summary>
        public IDocumentSession Session { get; }

        /// <summary>
        /// Returns the database context, which provides a handle to application wide DB
        /// activities such as committing any pending changes, beginning a transaction,
        /// rolling back a transaction, etc.
        /// </summary>
        public virtual ITransactionManager TransactionManager { get; }

        /// <summary>
        /// Finds all documents satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>
        /// Documents
        /// </returns>
        public IEnumerable<T> FindAll(Func<T, bool> where)
        {
            return this.FindAll().Where(where);
        }

        /// <summary>
        /// Finds single document satisfying given criteria.
        /// </summary>
        /// <param name="where">The criteria.</param>
        /// <returns>
        /// The document
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The query returned more than one result. Please refine your query.</exception>
        public T FindOne(Func<T, bool> where)
        {
            IEnumerable<T> foundList = this.FindAll(where);

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
        /// Finds the first document satisfying fiven criteria.
        /// </summary>
        /// <param name="where">The Criteria.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public T First(Func<T, bool> where)
        {
            return this.FindAll(where).First(where);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(T entity)
        {
            this.Session.Delete(entity);
        }

        /// <summary>
        /// Deletes the entity that matches the provided ID.
        /// </summary>
        /// <param name="id"></param>
        public void Delete(TIdT id)
        {
            if (id is ValueType)
            {
                this.Delete(this.Get(id));
            }
            else
            {
                this.Session.Advanced.Defer(new DeleteCommandData {Key = id.ToString()});
            }
        }

        /// <summary>
        /// Stores document in session.
        /// </summary>
        /// <param name="entity">The document.</param>
        /// <returns>Stored document.</returns>
        public T Save(T entity)
        {
            return this.SaveOrUpdate(entity);
        }

        /// <summary>
        /// Dissasociates the entity with the ORM so that changes made to it are not automatically
        /// saved to the database.
        /// </summary>
        /// <param name="entity"></param>
        public void Evict(T entity)
        {
            this.Session.Advanced.Evict(entity);
        }

        /// <summary>
        /// Returns the entity that matches the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// An entity or <c>null</c> if a row is not found matching the provided ID.
        /// </remarks>
        public T Get(TIdT id)
        {
            if (id is ValueType)
            {
                return this.Session.Load<T>(id as ValueType);
            }

            return this.Session.Load<T>(id.ToString());
        }

        /// <summary>
        /// Returns all of the items of a given type.
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAll()
        {
            return this.FindAll().ToList();
        }

        /// <summary>
        /// Loads all documents with given IDs.
        /// </summary>
        /// <param name="ids">Document IDs.</param>
        /// <returns>
        /// List of documents.
        /// </returns>
        public IList<T> GetAll(IEnumerable<TIdT> ids)
        {
            return this.Session.Load<T>(ids.Select(p => p.ToString()));
        }

        /// <summary>
        /// Saves or updates the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// For entities with automatically generated IDs, such as identity,
        /// <see cref="M:SharpArch.Domain.PersistenceSupport.IRepositoryWithTypedId`2.SaveOrUpdate(`0)" />  may be called when saving or updating an entity.
        /// </para>
        /// <para>
        /// Updating also allows you to commit changes to a detached object.
        /// More info may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </para>
        /// </remarks>
        public T SaveOrUpdate(T entity)
        {
            this.Session.Store(entity);
            return entity;
        }
    }
}
