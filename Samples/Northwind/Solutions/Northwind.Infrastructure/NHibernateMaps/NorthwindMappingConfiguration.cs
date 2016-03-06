namespace Northwind.Infrastructure.NHibernateMaps
{
    using System;
    using System.Linq;

    using FluentNHibernate;
    using FluentNHibernate.Automapping;

    using SharpArch.Domain.DomainModel;

    public class NorthwindMappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool AbstractClassIsLayerSupertype(Type type)
        {
            return type == typeof(EntityWithTypedId<>) || type == typeof(Entity);
        }

        public override bool IsId(Member member)
        {
            return member.Name == "Id";
        }

        public override bool ShouldMap(Type type)
        {
            return
                type.GetInterfaces().Any(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }

        public override bool ShouldMap(Member member)
        {
            return base.ShouldMap(member) && member.CanWrite;
        }
    }
}