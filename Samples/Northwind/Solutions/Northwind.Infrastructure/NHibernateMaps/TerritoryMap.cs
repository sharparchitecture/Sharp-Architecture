namespace Northwind.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Northwind.Domain;

    public class TerritoryMap : IAutoMappingOverride<Territory>
    {
        public void Override(AutoMapping<Territory> mapping)
        {
            // Evil assigned ID - use identity instead unless you're working with a legacy DB
            mapping.Id(x => x.Id, "TerritoryID").GeneratedBy.Assigned();

            mapping.Map(x => x.Description, "TerritoryDescription");

            mapping.References(x => x.RegionBelongingTo, "RegionID").Not.Nullable();

            mapping.HasManyToMany(x => x.Employees).Table("EmployeeTerritories").Inverse().ParentKeyColumn(
                "TerritoryID").ChildKeyColumn("EmployeeID").AsBag();
        }
    }
}