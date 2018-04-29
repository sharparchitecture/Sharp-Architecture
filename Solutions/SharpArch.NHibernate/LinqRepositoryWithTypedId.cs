namespace SharpArch.NHibernate
{
    using System.Linq;
    using JetBrains.Annotations;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Domain.Specifications;


    /// <summary>
    ///     LINQ extensions to NHibernate repository.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="SharpArch.NHibernate.NHibernateRepositoryWithTypedId{T, TId}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public class LinqRepositoryWithTypedId<T, TId> : NHibernateRepositoryWithTypedId<T, TId>, ILinqRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinqRepositoryWithTypedId{T, TId}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        public LinqRepositoryWithTypedId(INHibernateTransactionManager transactionManager) : base(transactionManager)
        { }

        /// <inheritdoc />
        public IQueryable<T> FindAll()
        {
            return Session.Query<T>();
        }

        /// <inheritdoc />
        public IQueryable<T> FindAll(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(Session.Query<T>());
        }

        /// <inheritdoc />
        public T FindOne(TId id)
        {
            return Session.Get<T>(id);
        }

        /// <inheritdoc />
        public T FindOne(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(Session.Query<T>()).SingleOrDefault();
        }
    }
}
