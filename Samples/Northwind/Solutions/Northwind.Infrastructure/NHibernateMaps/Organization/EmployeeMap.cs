namespace Northwind.Infrastructure.NHibernateMaps.Organization
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Northwind.Domain.Organization;

    public class EmployeeMap : IAutoMappingOverride<Employee>
    {
        public void Override(AutoMapping<Employee> mapping)
        {
            mapping.Id(x => x.Id, "EmployeeID").UnsavedValue(0).GeneratedBy.Identity();

            // No need to specify the column name when it's the same as the property name
            mapping.Map(x => x.FirstName);
            mapping.Map(x => x.LastName);
            mapping.Map(x => x.PhoneExtension, "Extension");

            mapping.HasManyToMany(x => x.Territories).Table("EmployeeTerritories").ParentKeyColumn("EmployeeID").
                ChildKeyColumn("TerritoryID").AsBag();
        }
    }
}