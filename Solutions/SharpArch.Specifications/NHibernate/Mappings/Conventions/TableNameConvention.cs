namespace SharpArch.Specifications.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;

    public class TableNameConvention : IClassConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IClassInstance instance)
        {
            instance.Table(instance.EntityType.Name);

        }
    }
}