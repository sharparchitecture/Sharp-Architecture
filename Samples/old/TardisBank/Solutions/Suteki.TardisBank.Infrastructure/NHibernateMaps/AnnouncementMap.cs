namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;
    using NHibernate.Type;
    using Suteki.TardisBank.Domain;

    public class AnnouncementMap : IAutoMappingOverride<Announcement>
    {
        public void Override(AutoMapping<Announcement> mapping)
        {
            mapping.Map(x => x.LastModifiedUtc).CustomType(typeof (UtcDateTimeType));
        }
    }
}