namespace SharpArch.Domain.DomainModel
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides a base class for your objects which will be persisted to the database.
    /// </summary>
    /// <remarks>
    ///     Benefits include the addition of an Id property along with a consistent manner for
    ///     comparing entities.
    ///     Since nearly all of the entities you create will have a type of int Id, this 
    ///     base class leverages this assumption. If you want an entity with a type other 
    ///     than int, such as string, then use <see cref="EntityWithTypedId{IdT}" /> instead.
    /// </remarks>
    [Serializable]
    [PublicAPI]
    public abstract class Entity : EntityWithTypedId<int>
    {
    }
}