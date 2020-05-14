namespace SharpArch.Testing.Helpers
{
    using System;
    using System.Reflection;
    using JetBrains.Annotations;
    using Domain.DomainModel;

    /// <summary>
    ///     For better data integrity, it is imperative that the <see cref="Entity{TId}.Id" />
    ///     property is read-only and set only by the ORM.  With that said, some unit tests need
    ///     Id set to a particular value; therefore, this utility enables that ability.  This class should
    ///     never be used outside of the testing project; instead, implement <see cref="IHasAssignedId{IdT}" /> to
    ///     expose a public setter.
    /// </summary>
    [PublicAPI]
    public static class EntityIdSetter
    {
        /// <summary>
        ///     Uses reflection to set the Id of a <see cref="Entity{TId}" />.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">Property with name 'Id' could not be found.</exception>
        public static void SetIdOf<TId>([NotNull] IEntity<TId> entity, TId id)
            where TId : IEquatable<TId>
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // Set the data property reflectively
            PropertyInfo idProperty = entity.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

            if (idProperty == null)
                throw new InvalidOperationException("Property with name 'Id' could not be found.");

            idProperty.SetValue(entity, id, null);
        }

        /// <summary>
        ///     Uses reflection to set the Id of a <see cref="Entity{TId}" />.
        /// </summary>
        public static IEntity<TId> SetIdTo<TId>(this IEntity<TId> entity, TId id)
            where TId : IEquatable<TId>
        {
            SetIdOf(entity, id);
            return entity;
        }
    }
}
