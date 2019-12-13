namespace SharpArch.RavenDb.Contracts.Repositories
{
    using JetBrains.Annotations;

    /// <summary>
    /// RavenDB repository for documents with primary key of type <see cref="int"/>.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IAsyncRepository{T}" />
    [PublicAPI]
    public interface IRavenDbRepository<T> : IRavenDbRepositoryWithTypedId<T, int>
        where T : class
    {
    }
}
