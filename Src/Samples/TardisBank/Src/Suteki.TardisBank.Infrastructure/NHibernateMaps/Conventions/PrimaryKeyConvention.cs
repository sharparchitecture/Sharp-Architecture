namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions;

using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;


[UsedImplicitly]
public class PrimaryKeyConvention : IIdConvention
{
    public void Apply(IIdentityInstance instance)
    {
        instance.Column(instance.EntityType.Name + "Id");
        instance.GeneratedBy.HiLo("10");
    }
}
