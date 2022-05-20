namespace Suteki.TardisBank.Infrastructure.NHibernateMaps;

using FluentNHibernate;
using FluentNHibernate.Automapping;
using SharpArch.Domain.DomainModel;


/// <inheritdoc />
public class AutomappingConfiguration : DefaultAutomappingConfiguration
{
    /// <inheritdoc />
    public override bool ShouldMap(Type type)
    {
        return type.GetInterfaces().Any(x => x == typeof(IEntity));
    }

    /// <inheritdoc />
    public override bool ShouldMap(Member member)
        // map only writable properties
        => base.ShouldMap(member) && member.CanWrite;

    /// <inheritdoc />
    public override bool AbstractClassIsLayerSupertype(Type type)
        => type == typeof(Entity<>);
}
