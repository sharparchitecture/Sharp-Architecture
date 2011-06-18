namespace SharpArch.Testing.NUnit.Helpers
{
    using System.Reflection;

    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;

    /// <summary>
    ///     For better data integrity, it is imperitive that the <see cref = "Entity.Id" />
    ///     property is read-only and set only by the ORM.  With that said, some unit tests need 
    ///     Id set to a particular value; therefore, this utility enables that ability.  This class should 
    ///     never be used outside of the testing project; instead, implement <see cref = "IHasAssignedId{IdT}" /> to 
    ///     expose a public setter.
    /// </summary>
    public static class EntityIdSetter
    {
        /// <summary>
        ///     Uses reflection to set the Id of a <see cref = "EntityWithTypedId{IdT}" />.
        /// </summary>
        public static void SetIdOf<TId>(IEntityWithTypedId<TId> entity, TId id)
        {
            // Set the data property reflectively
            var idProperty = entity.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

            Check.Ensure(idProperty != null, "idProperty could not be found");

            idProperty.SetValue(entity, id, null);
        }

        /// <summary>
        ///     Uses reflection to set the Id of a <see cref = "EntityWithTypedId{IdT}" />.
        /// </summary>
        public static IEntityWithTypedId<TId> SetIdTo<TId>(this IEntityWithTypedId<TId> entity, TId id)
        {
            SetIdOf(entity, id);
            return entity;
        }
    }
}