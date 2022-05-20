namespace SharpArch.NHibernate.FluentNHibernate
{
    using System;
    using System.Linq;
    using Domain.DomainModel;
    using global::FluentNHibernate.Automapping;
    using JetBrains.Annotations;


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
    /// <seealso cref="IEntity{TId}" />
    /// <seealso cref="ValueObject" />
    [PublicAPI]
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        /// <summary>
        ///     Checks if given type is S#Arch entity.
        /// </summary>
        public override bool ShouldMap(Type type)
        {
            return !type.IsNested && type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEntity<>));
        }

        /// <summary>
        ///     Marks all <see cref="ValueObject" /> descendants as components.
        ///     See https://martinfowler.com/eaaCatalog/valueObject.html
        /// </summary>
        public override bool IsComponent(Type type)
            => typeof(ValueObject).IsAssignableFrom(type);

        /// <summary>
        ///     Marks all abstract descendants of <see cref="Entity{TId}" /> as Layer Supertype.
        ///     See http://martinfowler.com/eaaCatalog/layerSupertype.html
        /// </summary>
        public override bool AbstractClassIsLayerSupertype(Type type)
            => type == typeof(Entity<>);
    }
}
