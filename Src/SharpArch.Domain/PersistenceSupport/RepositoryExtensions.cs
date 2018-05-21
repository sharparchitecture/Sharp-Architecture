namespace SharpArch.Domain.PersistenceSupport
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Repository extension methods.
    /// </summary>
    [PublicAPI]
    public static class RepositoryExtensions
    {
        /// <summary>
        ///     Saves the specified object to the repository and evicts it from the session.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="repository" /> is <see langword="null" />.</exception>
        public static void SaveAndEvict<T, TId>(this IRepositoryWithTypedId<T, TId> repository, T entity)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            repository.Evict(repository.Save(entity));
        }
    }
}
