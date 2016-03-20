namespace Northwind.Infrastructure.NHibernateMaps
{
    using FluentNHibernate.Mapping;

    using Northwind.Domain;

    public class CategoryMap : ClassMap<Category>
    {
        public CategoryMap()
        {
            this.Table("Categories");

            this.Id(x => x.Id, "CategoryID").UnsavedValue(0).GeneratedBy.Identity();

            this.Map(x => x.CategoryName, "CategoryName");
        }
    }
}