namespace SharpArch.NHibernate
{
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate.Contracts.Repositories;
    using System;

    /// <summary>
    ///     Since nearly all of the domain objects you create will have a type of int Id, this 
    ///     most freqently used base NHibernateRepository leverages this assumption.  If you want 
    ///     an entity with a type other than int, such as string, then use 
    ///     <see cref = "NHibernateRepositoryWithTypedId{T, IdT}" />.
    /// </summary>
    public class NHibernateRepository<T> : NHibernateRepositoryWithTypedId<T, Guid>, IRepository<T>
    {
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}