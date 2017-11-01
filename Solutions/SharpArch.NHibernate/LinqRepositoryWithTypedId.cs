namespace SharpArch.NHibernate
{
    using System.Linq;
    using global::NHibernate;
    using global::NHibernate.Linq;

    using Domain.PersistenceSupport;
    using Domain.Specifications;
    using JetBrains.Annotations;

    /// <summary>
    /// LINQ extensions to NHibernate repository.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="SharpArch.NHibernate.NHibernateRepositoryWithTypedId{T, TId}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public class LinqRepositoryWithTypedId<T, TId> : NHibernateRepositoryWithTypedId<T, TId>, ILinqRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqRepositoryWithTypedId{T, TId}"/> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <param name="session">The session.</param>
        public LinqRepositoryWithTypedId(ITransactionManager transactionManager, ISession session) : base(transactionManager, session)
        {
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
            return specification.SatisfyingElementsFrom(this.Session.Query<T>());
        }

        /// <summary>
        /// Finds an item by Id.
        /// </summary>
        /// <param name="id">The Id of the entity.</param>
        /// <returns>
        /// The matching item.
        /// </returns>
        public T FindOne(TId id)
        {
            return this.Session.Get<T>(id);
        }

        /// <summary>
        /// Finds an item by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>
        /// The matching item or <c>null</c>.
        /// </returns>
        public T FindOne(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(this.Session.Query<T>()).SingleOrDefault();
        }

    }
}