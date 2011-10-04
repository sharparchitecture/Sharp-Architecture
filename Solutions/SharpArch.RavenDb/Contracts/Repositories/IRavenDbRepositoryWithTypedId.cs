namespace SharpArch.RavenDb.Contracts.Repositories
{
    using System;
    using System.Collections.Generic;

    using SharpArch.Domain.PersistenceSupport;

    public interface IRavenDbRepositoryWithTypedId<T, TIdT> : IRepositoryWithTypedId<T, TIdT>
    {
        #region Public Methods

        IEnumerable<T> FindAll(Func<T, bool> where, bool waitForNonStaleResults = false);

        T FindOne(Func<T, bool> where, bool waitForNonStaleResults = false);

        T First(Func<T, bool> where, bool waitForNonStaleResults = false);

        IList<T> GetAll(IEnumerable<TIdT> ids);

        #endregion
    }
}