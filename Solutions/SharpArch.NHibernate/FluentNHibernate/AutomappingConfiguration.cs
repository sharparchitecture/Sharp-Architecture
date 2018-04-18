namespace SharpArch.NHibernate.FluentNHibernate
{
    using System;
    using System.Linq;
    using global::FluentNHibernate;
    using global::FluentNHibernate.Automapping;
    using SharpArch.Domain.DomainModel;

    /// <summary>
    ///     Applies custom mapping conventions to S#Arch entities.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             <description>Following rules are applied:</description>
    ///         </listheader>
    ///         <item>
    ///             <term>ID</term>
    ///             <description>Property with name <c>Id</c> will be mapped to entity ID.</description>
    ///         </item>
    ///         <item>
    ///             <term>Component</term>
    ///             <description><see cref="ValueObject" /> descendants will be mapped as Components.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="DefaultAutomappingConfiguration" />
    /// <seealso cref="IEntityWithTypedId{TId}" />
    /// <seealso cref="ValueObject" />
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        /// <summary>
        ///     Checks if given type is S#Arch entity.
        /// </summary>
        public override bool ShouldMap(Type type)
        {
            return !type.IsNested && type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }

        /// <summary>
        ///     Determines whether the specified type is component (value object).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool IsComponent(Type type)
        {
            return typeof(ValueObject).IsAssignableFrom(type);
        }

        /// <summary>
        ///     Determines whether the specified member is identifier.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        public override bool IsId(Member member)
        {
            return member.Name == "Id";
        }
    }
}
