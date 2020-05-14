namespace LogDb
{
    using System;
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Conventions;
    using SharpArch.Domain.DomainModel;
    using SharpArch.NHibernate.FluentNHibernate;
    using SharpArch.NHibernate.FluentNHibernate.Conventions;


    public class LogDbPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            var mappings = AutoMap.AssemblyOf<LogDbPersistenceModelGenerator>(new AutomappingConfiguration());
            mappings.IgnoreBase(typeof(Entity<>));
            mappings.Conventions.Setup(GetConventions());
            mappings.UseOverridesFromAssemblyOf<LogDbPersistenceModelGenerator>();

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
