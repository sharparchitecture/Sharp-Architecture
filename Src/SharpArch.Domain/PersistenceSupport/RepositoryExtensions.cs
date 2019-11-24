namespace SharpArch.Domain.PersistenceSupport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="repository" /> or <paramref name="entity" /> is
        ///     <see langword="null" />.
        /// </exception>
        public static async Task SaveAndEvictAsync<T, TId>(
            [NotNull] this IAsyncRepositoryWithTypedId<T, TId> repository, [NotNull] T entity, CancellationToken cancellationToken = default)
            where T : class
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var saved = await repository.SaveAsync(entity, CancellationToken.None).ConfigureAwait(false);
            await repository.EvictAsync(saved, cancellationToken).ConfigureAwait(false);
        }
    }
}
