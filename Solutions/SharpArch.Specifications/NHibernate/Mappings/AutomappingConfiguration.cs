namespace SharpArch.Specifications.NHibernate.Mappings
{
    using System;
    using System.Linq;

    using FluentNHibernate;
    using FluentNHibernate.Automapping;

    using global::SharpArch.Domain.DomainModel;

    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            return type.GetInterfaces().Any(x =>
                                         x.IsGenericType && 
                                         x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));

        }

        public override bool IsComponent(Type type)
        {
            return typeof(ValueObject).IsAssignableFrom(type);
        }

        public override bool IsId(Member member)
        {
            return member.Name == "Id";
        }
    }
}