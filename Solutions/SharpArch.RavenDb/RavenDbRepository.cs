namespace SharpArch.RavenDb
{
    using JetBrains.Annotations;
    using Raven.Client;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.RavenDb.Contracts.Repositories;

    /// <summary>
    /// RavenDB repository.
    /// This repository supports entities with primary key of type <see langword="int"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <seealso cref="int" />
    /// <seealso cref="SharpArch.RavenDb.Contracts.Repositories.IRavenDbRepository{T}" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepository{T}" />
    [PublicAPI]
    public class RavenDbRepository<T> : RavenDbRepositoryWithTypedId<T, int>,
        IRavenDbRepository<T>,
        ILinqRepository<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDbRepository{T}"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RavenDbRepository(IDocumentSession session) : base(session)
        {
        }
    }
}