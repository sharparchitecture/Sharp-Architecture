namespace SharpArch.Specifications.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;
    
    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(FluentNHibernate.Conventions.Instances.IOneToManyCollectionInstance instance)
        {
            instance.Key.Column(instance.EntityType.Name + "Id");
            instance.Cascade.AllDeleteOrphan();
            instance.Inverse();
        }
    }
}