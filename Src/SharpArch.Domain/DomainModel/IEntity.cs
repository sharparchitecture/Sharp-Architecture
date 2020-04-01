namespace SharpArch.Domain.DomainModel
{
    using System.Reflection;
    using JetBrains.Annotations;


    /// <summary>
    ///     Non-generic entity interface.
    /// </summary>
    [PublicAPI]
    public interface IEntity
    {
        /// <summary>
        ///     Returns entity identifier.
        /// </summary>
        /// <returns>Entity identifier or null for transient entities.</returns>
        /// <remarks>
        ///     Calling this method may result in boxing for entities with value type identifier.
        ///     Use <see cref="IEntityWithTypedId{TId}" /> where possible.
        /// </remarks>
        [CanBeNull]
        object GetId();

        /// <summary>
        ///     Returns the properties of the current object that make up the object's signature.
        /// </summary>
        /// <returns>A collection of <see cref="PropertyInfo" /> instances.</returns>
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
