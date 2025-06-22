namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;


public class MessageMap : IAutoMappingOverride<Message>
{
    public void Override(AutoMapping<Message> mapping)
    {
        mapping.References(m => m.User);
    }
}
