using System;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using SharpArch.Domain.DomainModel;
using SharpArch.NHibernate.FluentNHibernate;
using Suteki.TardisBank.Domain;
using Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions;

namespace Suteki.TardisBank.Infrastructure.NHibernateMaps
{
    /// <summary>
    ///     Generates the automapping for the domain assembly
    /// </summary>
    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            var mappings = AutoMap.AssemblyOf<Child>(new AutomappingConfiguration());
            mappings.IgnoreBase<Entity>();
            mappings.IgnoreBase(typeof(EntityWithTypedId<>));
            mappings.Conventions.Setup(GetConventions());
            mappings.UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();

            return mappings;
        }

        private static Action<IConventionFinder> GetConventions()
        {
            return c =>
            {
                c.Add<PrimaryKeyConvention>();
                c.Add<CustomForeignKeyConvention>();
                c.Add<HasManyConvention>();
                c.Add<TableNameConvention>();
                c.Add<ReferenceConvention>();
                c.Add<JoinedSubClassConvention>();
                c.Add<HasOneConvention>();
            };
        }
    }
}
