namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions
{
    #region Using Directives

    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;
    using Humanizer;

    #endregion

    public class HasManyConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Key.Column(instance.EntityType.Name + "Id");
            var parent = instance.Relationship.EntityType.Name.Pluralize();
            instance.Key.ForeignKey(
                $"FK_{instance.StringIdentifierForModel.Pluralize()}_For_{parent}");

            instance.Cascade.AllDeleteOrphan();
            instance.Inverse();
        }
    }
}