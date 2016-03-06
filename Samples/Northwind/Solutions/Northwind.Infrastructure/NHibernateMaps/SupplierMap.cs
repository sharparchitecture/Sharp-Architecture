namespace Northwind.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Automapping.Alterations;

    using Northwind.Domain;

    public class SupplierMap : IAutoMappingOverride<Supplier>
    {
        public void Override(AutoMapping<Supplier> mapping)
        {
            mapping.Not.LazyLoad();

            mapping.Id(x => x.Id, "SupplierID").UnsavedValue(0).GeneratedBy.Identity();

            mapping.Map(x => x.CompanyName);

            mapping.HasMany(x => x.Products).Inverse().KeyColumn("SupplierID").AsBag();
        }
    }
}