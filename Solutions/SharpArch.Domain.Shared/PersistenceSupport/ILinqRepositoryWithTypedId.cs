namespace SharpArch.Domain.PersistenceSupport
{
    using System.Linq;
    using JetBrains.Annotations;
    using Specifications;

    /// <summary>
    ///     Defines the public members of a LINQ supported repository.
    /// </summary>
    /// <remarks>
    ///     Defines a LINQ implementation of the Repository Pattern that takes in a Specification to
    ///     define the items that should be returned.
    /// </remarks>
    [PublicAPI]
    public interface ILinqRepositoryWithTypedId<T, in TId>: IRepositoryWithTypedId<T, TId>
    {
        /// <summary>
        ///     Finds an item by ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The matching item.</returns>
        [CanBeNull]
        T FindOne(TId id);

        /// <summary>
        ///     Finds an item by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>The matching item.</returns>
        [CanBeNull]
        T FindOne([NotNull] ILinqSpecification<T> specification);

        /// <summary>
        ///     Finds all items within the repository.
        /// </summary>
        /// <returns>All items in the repository.</returns>
        [NotNull]
        IQueryable<T> FindAll();

        /// <summary>
        ///     Finds all items by a specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>All matching items.</returns>
        [NotNull]
        IQueryable<T> FindAll([NotNull] ILinqSpecification<T> specification);
    }
}