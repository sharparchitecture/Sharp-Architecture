namespace Northwind.Infrastructure.NHibernateMaps
{
    using System;

    using FluentNHibernate.Automapping;
    using FluentNHibernate.Conventions;

    using Northwind.Domain;
    using Northwind.Infrastructure.NHibernateMaps.Conventions;

    using SharpArch.Domain.DomainModel;
    using SharpArch.NHibernate.FluentNHibernate;

    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            return
                AutoMap.AssemblyOf<Category>(new NorthwindMappingConfiguration()).Conventions.Setup(
                    this.GetConventions()).IgnoreBase<Entity>().IgnoreBase(typeof(EntityWithTypedId<>)).
                    UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();
        }

        private Action<IConventionFinder> GetConventions()
        {
            return c =>
                {
                    c.Add<PrimaryKeyConvention>();
                    c.Add<HasManyConvention>();
                    c.Add<TableNameConvention>();
                };
        }
    }
}