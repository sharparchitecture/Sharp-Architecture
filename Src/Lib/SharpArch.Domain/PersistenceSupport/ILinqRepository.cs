namespace SharpArch.Domain.PersistenceSupport;

using DomainModel;
using Specifications;


/// <summary>
///     Defines the public members of a LINQ supported repository.
/// </summary>
/// <remarks>
///     Defines a LINQ implementation of the Repository Pattern that takes in a Specification to
///     define the items that should be returned.
/// </remarks>
[PublicAPI]
public interface ILinqRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    ///     Finds an item by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching item.</returns>
    Task<TEntity?> FindOneAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an item by a specification.
    /// </summary>
    /// <param name="specification">The specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching item.</returns>
    Task<TEntity?> FindOneAsync(ILinqSpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns <see cref="IQueryable{T}" /> representing query for the entity.
    /// </summary>
    /// <returns>Query.</returns>
    IQueryable<TEntity> FindAll();

    /// <summary>
    ///     Returns query to filter entities by a specification.
    /// </summary>
    /// <param name="specification">The specification.</param>
    /// <returns>Query.</returns>
    IQueryable<TEntity> FindAll(ILinqSpecification<TEntity> specification);
}
