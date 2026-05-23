namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using Conventions;
using Domain;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using SharpArch.Domain.DomainModel;
using SharpArch.NHibernate.FluentNHibernate;


/// <summary>
///     Generates the auto-mapping for the domain assembly
/// </summary>
public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
{
    public AutoPersistenceModel Generate()
    {
        var mappings = AutoMap.AssemblyOf<Child>(new AutomappingConfiguration());
        mappings.IgnoreBase(typeof(Entity<>));
        mappings.Conventions.Setup(GetConventions());
        mappings.UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();

        return mappings;
    }

    static Action<IConventionFinder> GetConventions()
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
