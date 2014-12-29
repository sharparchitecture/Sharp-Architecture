namespace SharpArch.RavenDb.Contracts.Repositories
{
    using SharpArch.Domain.PersistenceSupport;

    public interface IRavenDbRepository<T> : IRavenDbRepositoryWithTypedId<T, int>, IRepository<T>
    {
    }
}