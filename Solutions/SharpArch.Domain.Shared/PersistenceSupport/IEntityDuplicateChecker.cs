namespace SharpArch.Domain.PersistenceSupport
{
    using DomainModel;
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines the public members of a class that checks an entity for duplicates.
    /// </summary>
    [PublicAPI]
    public interface IEntityDuplicateChecker
    {
        /// <summary>Returns a value indicating whether a duplicate of the specified <paramref name="entity"/> exists.</summary>
        /// <typeparam name="TId">The type of the ID that identifies the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.ArgumentNullException"><see paramref="entity" /> is null.</exception>
        /// <returns>
        /// <c>true</c> if a duplicate exists, <c>false</c> otherwise.
        /// </returns>
        bool DoesDuplicateExistWithTypedIdOf<TId>([NotNull] IEntityWithTypedId<TId> entity);
    }
}