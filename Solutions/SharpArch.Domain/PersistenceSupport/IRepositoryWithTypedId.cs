namespace SharpArch.Domain.PersistenceSupport
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Defines the public members of a class that implements the repository pattern for entities
    ///     of the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TId">The type of the entity ID.</typeparam>
    public interface IRepositoryWithTypedId<T, TId> : ICollection<T>, IQueryable<T>
    {
        /// <summary>
        ///     Get the specified entity
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="id">The entity to load</param>
        T this[TId id] { get; }
    }
}