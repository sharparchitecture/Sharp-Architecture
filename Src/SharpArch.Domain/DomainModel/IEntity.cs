namespace SharpArch.Domain.DomainModel
{
    using System;
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
        ///     Use <see cref="IEntity{TId}" /> where possible.
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

        /// <summary>
        ///     Returns the unproxied type of the current object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         When NHibernate proxies objects, it masks the type of the actual entity object.
        ///         This wrapper burrows into the proxied object to get its actual type.
        ///     </para>
        ///     <para>
        ///         Although this assumes NHibernate is being used, it doesn't require any NHibernate
        ///         related dependencies and has no bad side effects if NHibernate isn't being used.
        ///     </para>
        ///     <para>
        ///         Related discussion is at
        ///         http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        ///     </para>
        /// </remarks>
        Type GetTypeUnproxied();
    }

    /// <summary>
    ///     This serves as a base interface for <see cref="Entity{TId}" />.
    /// It also provides a simple means to develop your own base entity.
    /// </summary>
    [PublicAPI]
    public interface IEntity<out TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets the ID which uniquely identifies the entity instance within its type's bounds.
        /// </summary>
        TId Id { get; }
    }

}
