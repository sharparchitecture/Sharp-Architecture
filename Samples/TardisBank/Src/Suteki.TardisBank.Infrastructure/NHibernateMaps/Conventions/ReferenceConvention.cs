namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;
    using Humanizer;

    public class ReferenceConvention : IReferenceConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            instance.ForeignKey($"FK_{instance.EntityType.Name.Pluralize()}_Ref_{instance.Name.Pluralize()}");
        }
    }
}