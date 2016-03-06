namespace Northwind.Infrastructure.NHibernateMaps.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            instance.Column("Id");
            instance.UnsavedValue("0");

            // instance.GeneratedBy.HiLo("1000");
        }
    }
}