namespace MainDb
{
    using System;
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Conventions;
    using SharpArch.Domain.DomainModel;
    using SharpArch.NHibernate.FluentNHibernate;
    using SharpArch.NHibernate.FluentNHibernate.Conventions;


    public class MainDbPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            var mappings = AutoMap.AssemblyOf<MainDbPersistenceModelGenerator>(new AutomappingConfiguration());
            mappings.IgnoreBase(typeof(Entity<>));
            mappings.Conventions.Setup(GetConventions());
            mappings.UseOverridesFromAssemblyOf<MainDbPersistenceModelGenerator>();

            return mappings;
        }

        static Action<IConventionFinder> GetConventions()
        {
            return c =>
            {
                c.Add<PrimaryKeyConvention>();
                c.Add<ForeignKeyConvention>();
                c.Add<HasManyConvention>();
            };
        }
    }
}
