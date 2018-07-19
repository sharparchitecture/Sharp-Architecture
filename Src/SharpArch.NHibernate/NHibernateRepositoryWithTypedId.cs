namespace SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using global::NHibernate.Criterion;
    using JetBrains.Annotations;
    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate.Contracts.Repositories;


    /// <summary>
    ///     Provides a fully loaded DAO which may be created in a few ways including:
    ///     * Direct instantiation; e.g., new GenericDao&lt;Customer, string&gt;
    ///     * Spring configuration; e.g.,
    ///     <object id="CustomerDao"
    ///         type="SharpArch.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, SharpArch.Data" autowire="byName" />
    /// </summary>
    [PublicAPI]
    public class NHibernateRepositoryWithTypedId<T, TId> : INHibernateRepositoryWithTypedId<T, TId>,
        IAsyncNHibernateRepositoryWithTypedId<T, TId>

    {
        /// <summary>
        ///     Gets NHibernate session.
        /// </summary>
        protected ISession Session => TransactionManager.Session;

        /// <summary>
        ///     Returns the database context, which provides a handle to application wide DB
        ///     activities such as committing any pending changes, beginning a transaction,
        ///     rolling back a transaction, etc.
        /// </summary>
        protected INHibernateTransactionManager TransactionManager { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NHibernateRepositoryWithTypedId{T, TId}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public NHibernateRepositoryWithTypedId(
            [NotNull] INHibernateTransactionManager transactionManager)
        {
            TransactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        }

        /// <inheritdoc />
        public Task<IList<T>> FindAllAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (propertyValuePairs == null) throw new ArgumentNullException(nameof(propertyValuePairs));
            if (propertyValuePairs.Count == 0)
                throw new ArgumentException("No properties specified. Please specify at least one property/value pair.",
                    nameof(propertyValuePairs));

            ICriteria criteria = Session.CreateCriteria(typeof(T));

            foreach (string key in propertyValuePairs.Keys) {
                criteria.Add(propertyValuePairs[key] != null
                    ? Restrictions.Eq(key, propertyValuePairs[key])
                    : Restrictions.IsNull(key));
            }

            return criteria.ListAsync<T>(cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<T>> FindAllAsync(
            T exampleInstance, string[] propertiesToExclude,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude)
                example.ExcludeProperty(propertyToExclude);

            criteria.Add(example);

            return criteria.ListAsync<T>(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> FindOneAsync(
            IReadOnlyDictionary<string, object> propertyValuePairs,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<T> foundList = await FindAllAsync(propertyValuePairs, cancellationToken).ConfigureAwait(false);
            if (foundList.Count > 1)
                throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1 ? foundList[0] : default(T);
        }

        /// <inheritdoc />
        public async Task<T> FindOneAsync(
            T exampleInstance, string[] propertiesToExclude,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<T> foundList = await FindAllAsync(exampleInstance, propertiesToExclude, cancellationToken).ConfigureAwait(false);
            if (foundList.Count > 1)
                throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1
                ? foundList[0]
                : default(T);
        }

        /// <inheritdoc />
        public Task<T> GetAsync(
            TId id, Enums.LockMode lockMode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync<T>(id, ConvertFrom(lockMode), cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> LoadAsync(TId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> LoadAsync(
            TId id, Enums.LockMode lockMode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync<T>(id, ConvertFrom(lockMode), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public Task<T> GetAsync(TId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<T>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            return criteria.ListAsync<T>(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> SaveAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var res = await Session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
            return (T) res;
        }

        /// <inheritdoc />
        public async Task<T> SaveOrUpdateAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <inheritdoc />
        public Task EvictAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.EvictAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.DeleteAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity != null)
                await DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        ITransactionManager IAsyncRepositoryWithTypedId<T, TId>.TransactionManager => TransactionManager;

        ITransactionManager IRepositoryWithTypedId<T, TId>.TransactionManager => TransactionManager;

        /// <summary>
        ///     Dissasociates the entity with the ORM so that changes made to it are not automatically
        ///     saved to the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>
        ///     In NHibernate this removes the entity from current session cache.
        ///     More details may be found at http://www.hibernate.org/hib_docs/nhibernate/html_single/#performance-sessioncache.
        /// </remarks>
        public virtual void Evict(T entity)
        {
            Session.Evict(entity);
        }

        /// <summary>
        ///     Looks for zero or more instances using the example provided.
        /// </summary>
        /// <param name="exampleInstance"></param>
        /// <param name="propertiesToExclude"></param>
        /// <returns></returns>
        public virtual IList<T> FindAll(T exampleInstance, params string[] propertiesToExclude)
        {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude)
                example.ExcludeProperty(propertyToExclude);
            criteria.Add(example);

            return criteria.List<T>();
        }

        /// <summary>
        ///     Looks for zero or more instances using the properties provided.
        ///     The key of the collection should be the property name and the value should be
        ///     the value of the property to filter by.
        /// </summary>
        /// <param name="propertyValuePairs"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">No properties specified. Please specify at least one property/value pair.</exception>
        public virtual IList<T> FindAll([NotNull] IReadOnlyDictionary<string, object> propertyValuePairs)
        {
            if (propertyValuePairs == null) throw new ArgumentNullException(nameof(propertyValuePairs));
            if (propertyValuePairs.Count == 0)
                throw new ArgumentException("No properties specified. Please specify at least one property/value pair.",
                    nameof(propertyValuePairs));

            ICriteria criteria = Session.CreateCriteria(typeof(T));
            foreach (string key in propertyValuePairs.Keys) {
                criteria.Add(propertyValuePairs[key] != null
                    ? Restrictions.Eq(key, propertyValuePairs[key])
                    : Restrictions.IsNull(key));
            }

            return criteria.List<T>();
        }

        /// <summary>
        ///     Looks for a single instance using the example provided.
        /// </summary>
        /// <param name="exampleInstance"></param>
        /// <param name="propertiesToExclude"></param>
        /// <returns></returns>
        /// <exception cref="NonUniqueResultException"></exception>
        public virtual T FindOne(T exampleInstance, params string[] propertiesToExclude)
        {
            IList<T> foundList = FindAll(exampleInstance, propertiesToExclude);
            if (foundList.Count > 1)
                throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1 ? foundList[0] : default(T);
        }

        /// <summary>
        ///     Looks for a single instance using the property/values provided.
        /// </summary>
        /// <param name="propertyValuePairs"></param>
        /// <returns></returns>
        /// <exception cref="NonUniqueResultException"></exception>
        public virtual T FindOne(IReadOnlyDictionary<string, object> propertyValuePairs)
        {
            IList<T> foundList = FindAll(propertyValuePairs);
            if (foundList.Count > 1)
                throw new NonUniqueResultException(foundList.Count);

            return foundList.Count == 1 ? foundList[0] : default(T);
        }

        /// <summary>
        ///     Returns null if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        public virtual T Get(TId id, Enums.LockMode lockMode)
        {
            return Session.Get<T>(id, ConvertFrom(lockMode));
        }

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T Load(TId id)
        {
            return Session.Load<T>(id);
        }

        /// <summary>
        ///     Throws an exception if a row is not found matching the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        public virtual T Load(TId id, Enums.LockMode lockMode)
        {
            return Session.Load<T>(id, ConvertFrom(lockMode));
        }

        /// <summary>
        ///     For entities that have assigned Id's, you must explicitly call Save to add a new one.
        ///     See http://www.hibernate.org/hib_docs/nhibernate/html_single/#mapping-declaration-id-assigned.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Saved entity instance.</returns>
        public virtual T Save(T entity)
        {
            Session.Save(entity);
            return entity;
        }

        /// <summary>
        ///     For entities that have assigned Id's, you should explicitly call Update to update an existing one.
        ///     Updating also allows you to commit changes to a detached object.  More info may be found at:
        ///     http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Entity instance.</returns>
        public virtual T Update(T entity)
        {
            Session.Update(entity);
            return entity;
        }

        /// <summary>
        ///     Deletes the specified entity.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(T entity)
        {
            Session.Delete(entity);
        }

        /// <summary>
        ///     Deletes the entity that matches the provided ID.
        /// </summary>
        /// <param name="id"></param>
        public void Delete(TId id)
        {
            T entity = Get(id);

            if (entity != null) {
                Delete(entity);
            }
        }

        /// <summary>
        ///     Returns the entity that matches the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        ///     An entity or <c>null</c> if a row is not found matching the provided ID.
        /// </remarks>
        public virtual T Get(TId id)
        {
            return Session.Get<T>(id);
        }

        /// <summary>
        ///     Returns all of the items of a given type.
        /// </summary>
        /// <returns>
        ///     All entities from database.
        /// </returns>
        public virtual IList<T> GetAll()
        {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
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

            Session.SaveOrUpdate(entity);
            return entity;
        }

        /// <summary>
        ///     Translates a domain layer lock mode into an NHibernate lock mode via reflection.  This is
        ///     provided to facilitate developing the domain layer without a direct dependency on the
        ///     NHibernate assembly.
        /// </summary>
        static LockMode ConvertFrom(Enums.LockMode lockMode)
        {
            switch (lockMode) {
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
                        "The provided lock mode , '" + lockMode +
                        ",' could not be translated into an NHibernate.LockMode. " +
                        "This is probably because NHibernate was updated and now has different lock modes which are out of synch " +
                        "with the lock modes maintained in the domain layer.");
            }
        }
    }
}
