namespace SharpArch.Specifications.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;

    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IIdentityInstance instance)
        {
            instance.Column(instance.EntityType.Name + "Id");
        }
    }
}