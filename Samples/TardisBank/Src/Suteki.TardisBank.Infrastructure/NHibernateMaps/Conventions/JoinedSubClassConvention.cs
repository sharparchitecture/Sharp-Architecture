namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions;

using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;


public class JoinedSubClassConvention : IJoinedSubclassConvention
{
    public void Apply(IJoinedSubclassInstance instance)
    {
        // pluralize table names
        instance.Table(instance.EntityType.Name.Pluralize());

        if (instance.Type.BaseType != null)
            instance.Key.ForeignKey(
                $"FK_{instance.EntityType.Name.Pluralize()}_Join_{instance.Type.BaseType.Name.Pluralize()}");
    }
}
