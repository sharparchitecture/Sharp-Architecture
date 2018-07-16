namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Suteki.TardisBank.Domain;

    public class ChildMap : IAutoMappingOverride<Child>
    {
        public void Override(AutoMapping<Child> mapping)
        {
            mapping.References(c => c.Account).Cascade.All();
        }
    }
}