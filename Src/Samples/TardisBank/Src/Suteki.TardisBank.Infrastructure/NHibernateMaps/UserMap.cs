namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class UserMap : IAutoMappingOverride<User>
{
    public void Override(AutoMapping<User> mapping)
    {
        mapping.HasMany(u => u.Messages).Cascade.AllDeleteOrphan().Inverse();
    }
}
