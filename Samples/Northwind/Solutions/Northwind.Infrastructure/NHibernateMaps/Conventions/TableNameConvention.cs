namespace Northwind.Infrastructure.NHibernateMaps.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    using inflector_extension;

    public class TableNameConvention : IClassConvention
    {
        public void Apply(IClassInstance instance)
        {
            instance.Table(instance.EntityType.Name.InflectTo().Pluralized);
        }
    }
}