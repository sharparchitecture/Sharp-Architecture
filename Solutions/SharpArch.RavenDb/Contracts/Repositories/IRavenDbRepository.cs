namespace SharpArch.RavenDb.Contracts.Repositories
{
    public interface IRavenDbRepository<T> : IRavenDbRepositoryWithTypedId<T, string>
    {
    }
}