namespace SharpArch.RavenDb.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;

    using Raven.Client;

    using SharpArch.Domain.PersistenceSupport;

    public interface IRavenDbRepositoryWithTypedId<T, TIdT> : IRepositoryWithTypedId<T, TIdT>
    {
        IDocumentSession Session { get; }

        IEnumerable<T> FindAll(Func<T, bool> where);

        T FindOne(Func<T, bool> where);

        T First(Func<T, bool> where);

        IList<T> GetAll(IEnumerable<TIdT> ids);
    }
}