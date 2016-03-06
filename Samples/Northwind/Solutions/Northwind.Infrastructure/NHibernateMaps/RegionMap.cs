namespace Northwind.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Northwind.Domain;

    public class RegionMap : IAutoMappingOverride<Region>
    {
        public void Override(AutoMapping<Region> mapping)
        {
            // Why they didn't make this plural, when every other table is, is beyond me
            mapping.Table("Region");

            // This seems to be a reference type in Northwind, so let's make it read only 
            mapping.ReadOnly();

            mapping.Id(x => x.Id, "RegionID").UnsavedValue(0).GeneratedBy.Assigned();

            mapping.Map(x => x.Description, "RegionDescription");
        }
    }
}