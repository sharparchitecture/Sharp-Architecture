namespace SharpArch.NHibernate
{
    using SharpArch.Domain.PersistenceSupport;

    public class LinqRepository<T> : LinqRepositoryWithTypedId<T, int>, ILinqRepository<T>
    {
    }
}