namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class TransactionMap : IAutoMappingOverride<Transaction>
{
    public void Override(AutoMapping<Transaction> mapping)
    {
        mapping.References(p => p.Account);
    }
}
