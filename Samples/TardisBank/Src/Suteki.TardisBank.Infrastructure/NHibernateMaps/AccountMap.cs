namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class AccountMap : IAutoMappingOverride<Account>
{
    public void Override(AutoMapping<Account> mapping)
    {
        mapping.HasMany(a => a.PaymentSchedules).Cascade.AllDeleteOrphan().Inverse();
        mapping.HasMany(a => a.Transactions).Cascade.AllDeleteOrphan().Inverse();
    }
}
