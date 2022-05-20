namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions;

using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;


/// <summary>
///     Pluralize table name.
/// </summary>
public class TableNameConvention : IClassConvention
{
    public void Apply(IClassInstance instance)
    {
        instance.Table(instance.EntityType.Name.Pluralize());
    }
}
