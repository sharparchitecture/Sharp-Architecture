namespace SharpArch.NHibernate
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using Domain.Specifications;
    using global::NHibernate.Linq;
    using JetBrains.Annotations;


    /// <summary>
    ///     LINQ extensions to NHibernate repository.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <seealso cref="SharpArch.NHibernate.NHibernateRepositoryWithTypedId{T, TId}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepositoryWithTypedId{T, TId}" />
    [PublicAPI]
    public class LinqRepositoryWithTypedId<T, TId> : NHibernateRepositoryWithTypedId<T, TId>, ILinqRepositoryWithTypedId<T, TId>
        where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinqRepositoryWithTypedId{T, TId}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        public LinqRepositoryWithTypedId(INHibernateTransactionManager transactionManager)
            : base(transactionManager)
        {
        }

        /// <inheritdoc />
        public Task<T> FindOneAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Session.GetAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> FindOneAsync(ILinqSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return specification.SatisfyingElementsFrom(Session.Query<T>()).SingleOrDefaultAsync(cancellationToken);
        }

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
    }
}
