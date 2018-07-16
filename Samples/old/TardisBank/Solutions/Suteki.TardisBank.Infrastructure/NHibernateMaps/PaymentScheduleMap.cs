namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Suteki.TardisBank.Domain;

    public class PaymentScheduleMap : IAutoMappingOverride<PaymentSchedule>
    {
        public void Override(AutoMapping<PaymentSchedule> mapping)
        {
            mapping.References(p => p.Account);
        }
    }
}