namespace SharpArch.RavenDb
{
    using Contracts.Repositories;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Raven.Client.Documents.Session;


    /// <summary>
    ///     RavenDB repository.
    ///     This repository supports entities with primary key of type <see langword="int" />.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <seealso cref="int" />
    /// <seealso cref="SharpArch.RavenDb.Contracts.Repositories.IRavenDbRepository{T}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepository{T}" />
    [PublicAPI]
    public class RavenDbRepository<T> : RavenDbRepositoryWithTypedId<T, int>,
        IRavenDbRepository<T>,
        ILinqRepository<T>
        where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RavenDbRepository{T}" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepository(IAsyncDocumentSession session)
            : base(session)
        {
        }
    }
}
