namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class PaymentScheduleMap : IAutoMappingOverride<PaymentSchedule>
{
    public void Override(AutoMapping<PaymentSchedule> mapping)
    {
        mapping.References(p => p.Account);
    }
}
