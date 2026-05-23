namespace SharpArch.Domain.PersistenceSupport;

using DomainModel;


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
    public static async Task SaveAndEvictAsync<TEntity, TId>(
        this IRepository<TEntity, TId> repository, TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TId>
        where TId : IEquatable<TId>
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var saved = await repository.SaveAsync(entity, CancellationToken.None).ConfigureAwait(false);
        await repository.EvictAsync(saved, cancellationToken).ConfigureAwait(false);
    }
}
