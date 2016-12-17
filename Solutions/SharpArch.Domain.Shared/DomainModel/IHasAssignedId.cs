namespace SharpArch.Domain.DomainModel
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines the public members of a class that supports setting an assigned ID of an object.
    /// </summary>
    /// <typeparam name="TId">The type of the ID.</typeparam>
    [PublicAPI]
    public interface IHasAssignedId<in TId>
    {
        /// <summary>
        ///     Sets the assigned ID of an object.
        /// </summary>
        /// <remarks>
        ///     This is not part of <see cref="Entity" /> since most entities do not have assigned
        ///     IDs and since business rules will certainly vary as to what constitutes a valid,
        ///     assigned ID for one object but not for another.
        /// </remarks>
        void SetAssignedIdTo(TId assignedId);
    }
}