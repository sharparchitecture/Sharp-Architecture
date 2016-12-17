namespace SharpArch.Domain.PersistenceSupport
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides a standard interface for DAOs which are data-access mechanism agnostic.
    /// </summary>
    /// <remarks>
    ///     Since nearly all of the domain objects you create will have a type of int ID, this 
    ///     base DAO leverages this assumption. If you want an entity with a type 
    ///     other than int, such as string, then use <see cref="IRepositoryWithTypedId{T, IdT}" />.
    /// </remarks>
    [PublicAPI]
    public interface IRepository<T> : IRepositoryWithTypedId<T, int>
    {
    }
}