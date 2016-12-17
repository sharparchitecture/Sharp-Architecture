namespace SharpArch.Domain.PersistenceSupport
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines the public members of a LINQ supported repository.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    [PublicAPI]
    public interface ILinqRepository<T> : ILinqRepositoryWithTypedId<T, int>
    {
    }
}