namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    public class HasOneConvention : IHasOneConvention
    {
        public void Apply(IOneToOneInstance instance)
        {
            instance.ForeignKey($"FK_{instance.EntityType.Name}_To_{instance.Name}");
        }
    }
}