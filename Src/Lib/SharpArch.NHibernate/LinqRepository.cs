namespace SharpArch.NHibernate;

using Domain.DomainModel;
using Domain.PersistenceSupport;
using Domain.Specifications;
using global::NHibernate.Linq;


/// <summary>
///     LINQ extensions to NHibernate repository.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <seealso cref="NHibernateRepository{TEntity,TId}" />
/// <seealso cref="ILinqRepository{T,TId}" />
[PublicAPI]
public class LinqRepository<TEntity, TId> : NHibernateRepository<TEntity, TId>, ILinqRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LinqRepository{T,TId}" /> class.
    /// </summary>
    /// <param name="transactionManager">The transaction manager.</param>
    public LinqRepository(INHibernateTransactionManager transactionManager)
        : base(transactionManager)
    {
    }

    /// <inheritdoc />
    public Task<TEntity?> FindOneAsync(TId id, CancellationToken cancellationToken = default)
        => Session.GetAsync<TEntity?>(id, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> FindOneAsync(ILinqSpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => specification.SatisfyingElementsFrom(Session.Query<TEntity>()).SingleOrDefaultAsync(cancellationToken)!;

    /// <inheritdoc />
    public IQueryable<TEntity> FindAll()
        => Session.Query<TEntity>();

    /// <inheritdoc />
    public IQueryable<TEntity> FindAll(ILinqSpecification<TEntity> specification)
        => specification.SatisfyingElementsFrom(Session.Query<TEntity>());
}
