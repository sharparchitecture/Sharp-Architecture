namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    using System.Linq;
    using FluentNHibernate;
    using FluentNHibernate.Automapping;
    using SharpArch.Domain.DomainModel;


    /// <inheritdoc />
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        /// <inheritdoc />
        public override bool ShouldMap(System.Type type)
        {
            return type.GetInterfaces().Any(x =>
                 x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }

        /// <inheritdoc />
        public override bool ShouldMap(Member member)
        {
            // map only writable properties
            return base.ShouldMap(member) && member.CanWrite;
        }

        /// <inheritdoc />
        public override bool AbstractClassIsLayerSupertype(System.Type type)
        {
            return type == typeof(EntityWithTypedId<>) || type == typeof(Entity);
        }

    }
}
