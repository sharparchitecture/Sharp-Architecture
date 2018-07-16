namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Suteki.TardisBank.Domain;

    public class TransactionMap : IAutoMappingOverride<Transaction>
    {
        public void Override(AutoMapping<Transaction> mapping)
        {
            mapping.References(p => p.Account);
        }
    }
}