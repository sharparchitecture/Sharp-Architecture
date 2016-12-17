namespace SharpArch.Domain.DomainModel
{
    using System.Reflection;
    using JetBrains.Annotations;

    /// <summary>
    ///     This serves as a base interface for <see cref="EntityWithTypedId{TId}" /> and 
    ///     <see cref="Entity" />. It also provides a simple means to develop your own base entity.
    /// </summary>
    [PublicAPI]
    public interface IEntityWithTypedId<out TId>
    {
        /// <summary>
        ///     Gets the ID which uniquely identifies the entity instance within its type's bounds.
        /// </summary>
        TId Id { get; }

        /// <summary>
        ///     Returns the properties of the current object that make up the object's signature.
        /// </summary>
        /// <returns>A collection of <see cref="PropertyInfo"/> instances.</returns>
        PropertyInfo[] GetSignatureProperties();

        /// <summary>
        ///     Returns a value indicating whether the current object is transient.
        /// </summary>
        /// <remarks>
        ///     Transient objects are not associated with an item already in storage. For instance,
        ///     a Customer is transient if its ID is 0.  It's virtual to allow NHibernate-backed 
        ///     objects to be lazily loaded.
        /// </remarks>
        bool IsTransient();
    }
}