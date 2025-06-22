namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class ChildMap : IAutoMappingOverride<Child>
{
    public void Override(AutoMapping<Child> mapping)
    {
        mapping.References(c => c.Account).Cascade.All();
    }
}
