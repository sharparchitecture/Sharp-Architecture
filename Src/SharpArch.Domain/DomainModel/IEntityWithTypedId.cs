namespace SharpArch.Domain.DomainModel
{
    using System;
    using JetBrains.Annotations;


    /// <summary>
    ///     This serves as a base interface for <see cref="EntityWithTypedId{TId}" /> and
    ///     <see cref="Entity" />. It also provides a simple means to develop your own base entity.
    /// </summary>
    [PublicAPI]
    public interface IEntityWithTypedId<out TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets the ID which uniquely identifies the entity instance within its type's bounds.
        /// </summary>
        TId Id { get; }
    }
}
