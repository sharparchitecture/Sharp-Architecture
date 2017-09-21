using System;

namespace SharpArch.NHibernate
{
    using System.Collections.Generic;
    using Contracts.Repositories;

    using Domain;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;
    
    using global::NHibernate;
    using global::NHibernate.Criterion;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides a fully loaded DAO which may be created in a few ways including:
    ///     * Direct instantiation; e.g., new GenericDao&lt;Customer, string&gt;
    ///     * Spring configuration; e.g., <object id = "CustomerDao" type = "SharpArch.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, SharpArch.Data" autowire = "byName" />
    /// </summary>
    [PublicAPI]
    public class NHibernateRepositoryWithTypedId<T, TId> : INHibernateRepositoryWithTypedId<T, TId>
    {
        #region Constants and Fields

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateRepositoryWithTypedId{T, TId}"/> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public NHibernateRepositoryWithTypedId([NotNull] ITransactionManager transactionManager,
            [NotNull] ISession session)
        {
            if (transactionManager == null) throw new ArgumentNullException(nameof(transactionManager));
            if (session == null) throw new ArgumentNullException(nameof(session));

            this.TransactionManager = transactionManager;
            this.Session = session;
        }

        #region Properties

        /// <summary>
        /// Returns the database context, which provides a handle to application wide DB
        /// activities such as committing any pending changes, beginning a transaction,
        /// rolling back a transaction, etc.
        /// </summary>
        public virtual ITransactionManager TransactionManager { get; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        protected virtual ISession Session { get; }

        #endregion

        #region Implemented Interfaces

        #region INHibernateRepositoryWithTypedId<T,TId>

        /// <summary>
        /// Dissasociates the entity with the ORM so that changes made to it are not automatically
        /// saved to the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>
        /// In NHibernate this removes the entity from current session cache.
        /// More details may be found at http://www.hibernate.org/hib_docs/nhibernate/html_single/#performance-sessioncache.
        /// </remarks>
        public virtual void Evict(T entity)
        {
            this.Session.Evict(entity);
        }

        /// <summary>
        /// Looks for zero or more instances using the example provided.
        /// </summary>
        /// <param name="exampleInstance"></param>
        /// <param name="propertiesToExclude"></param>
        /// <returns></returns>
        public virtual IList<T> FindAll(T exampleInstance, params string[] propertiesToExclude)
        {
            ICriteria criteria = this.Session.CreateCriteria(typeof(T));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude)
            {
                example.ExcludeProperty(propertyToExclude);
            }

            criteria.Add(example);

            return criteria.List<T>();
        }

        /// <summary>
        /// Looks for zero or more instances using the properties provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by.
        /// </summary>
        /// <param name="propertyValuePairs"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">No properties specified. Please specify at least one property/value pair.</exception>
        public virtual IList<T> FindAll([NotNull] IDictionary<string, object> propertyValuePairs)
        {
            if (propertyValuePairs == null) throw new ArgumentNullException(nameof(propertyValuePairs));
            if (propertyValuePairs.Count == 0) throw new ArgumentException("No properties specified. Please specify at least one property/value pair.", nameof(propertyValuePairs));

            ICriteria criteria = this.Session.CreateCriteria(typeof(T));

            foreach (string key in propertyValuePairs.Keys)
            {
                criteria.Add(propertyValuePairs[key] != null ? Restrictions.Eq(key, propertyValuePairs[key]) : Restrictions.IsNull(key));
            }

            return criteria.List<T>();
        }

        /// <summary>
        /// Looks for a single instance using the example provided.
        /// </summary>
        /// <param name="exampleInstance"></param>
        /// <param name="propertiesToExclude"></param>
        /// <returns></returns>
        /// <exception cref="NonUniqueResultException"></exception>
        public virtual T FindOne(T exampleInstance, params string[] propertiesToExclude)
        {
            IList<T> foundList = this.FindAll(exampleInstance, propertiesToExclude);

            if (foundList.Count > 1)
            {
                throw new NonUniqueResultException(foundList.Count);
            }

            if (foundList.Count == 1)
            {
                return foundList[0];
            }

            return default(T);
        }

        /// <summary>
        /// Looks for a single instance using the property/values provided.
        /// </summary>
        /// <param name="propertyValuePairs"></param>
        /// <returns></returns>
        /// <exception cref="NonUniqueResultException"></exception>
        public virtual T FindOne(IDictionary<string, object> propertyValuePairs)
        {
            IList<T> foundList = this.FindAll(propertyValuePairs);

            if (foundList.Count > 1)
            {
                throw new NonUniqueResultException(foundList.Count);
            }

            if (foundList.Count == 1)
            {
                return foundList[0];
            }

            return default(T);
        }

        /// <summary>
        /// Returns null if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        public virtual T Get(TId id, Enums.LockMode lockMode)
        {
            return this.Session.Get<T>(id, ConvertFrom(lockMode));
        }

        /// <summary>
        /// Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T Load(TId id)
        {
            return this.Session.Load<T>(id);
        }

        /// <summary>
        /// Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        public virtual T Load(TId id, Enums.LockMode lockMode)
        {
            return this.Session.Load<T>(id, ConvertFrom(lockMode));
        }

        /// <summary>
        /// For entities that have assigned Id's, you must explicitly call Save to add a new one.
        /// See http://www.hibernate.org/hib_docs/nhibernate/html_single/#mapping-declaration-id-assigned.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Saved entity instance.</returns>
        public virtual T Save(T entity)
        {
            this.Session.Save(entity);
            return entity;
        }


        /// <summary>
        /// For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        /// Updating also allows you to commit changes to a detached object.  More info may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Entity instance.</returns>
        public virtual T Update(T entity)
        {
            this.Session.Update(entity);
            return entity;
        }

        #endregion

        #region IRepositoryWithTypedId<T,TId>

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(T entity)
        {
            this.Session.Delete(entity);
        }

        /// <summary>
        /// Deletes the entity that matches the provided ID.
        /// </summary>
        /// <param name="id"></param>
        public void Delete(TId id)
        {
            T entity = this.Get(id);

            if (entity != null)
            {
                this.Delete(entity);
            }
        }

        /// <summary>
        /// Returns the entity that matches the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// An entity or <c>null</c> if a row is not found matching the provided ID.
        /// </remarks>
        public virtual T Get(TId id)
        {
            return this.Session.Get<T>(id);
        }

        /// <summary>
        /// Returns all of the items of a given type.
        /// </summary>
        /// <returns>
        /// All entities from database.
        /// </returns>
        public virtual IList<T> GetAll()
        {
            ICriteria criteria = this.Session.CreateCriteria(typeof(T));
            return criteria.List<T>();
        }

        /// <summary>
        ///     Although SaveOrUpdate _can_ be invoked to update an object with an assigned Id, you are 
        ///     hereby forced instead to use Save/Update for better clarity.
        /// </summary>
        public virtual T SaveOrUpdate(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (!(entity is IHasAssignedId<TId>))
                throw new InvalidOperationException(
                    "For better clarity and reliability, Entities with an assigned Id must call Save() or Update().");

            this.Session.SaveOrUpdate(entity);
            return entity;
        }

        #endregion

        #endregion

        #region Methods


        /// <summary>
        ///     Translates a domain layer lock mode into an NHibernate lock mode via reflection.  This is 
        ///     provided to facilitate developing the domain layer without a direct dependency on the 
        ///     NHibernate assembly.
        /// </summary>
        private static LockMode ConvertFrom(Enums.LockMode lockMode)
        {
            switch (lockMode)
            {
                case Enums.LockMode.None:
                    return LockMode.None;
                case Enums.LockMode.Read:
                    return LockMode.Read;
                case Enums.LockMode.Upgrade:
                    return LockMode.Upgrade;
                case Enums.LockMode.UpgradeNoWait:
                    return LockMode.UpgradeNoWait;
                case Enums.LockMode.Write:
                    return LockMode.Write;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lockMode), lockMode,
                        "The provided lock mode , '" + lockMode + ",' could not be translated into an NHibernate.LockMode. " +
                        "This is probably because NHibernate was updated and now has different lock modes which are out of synch " +
                        "with the lock modes maintained in the domain layer.");
            }
        }

        #endregion
    }
}