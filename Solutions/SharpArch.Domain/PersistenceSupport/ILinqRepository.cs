namespace SharpArch.Domain.PersistenceSupport
{
    public interface ILinqRepository<T> : ILinqRepositoryWithTypedId<T, int>
    {
    }
}