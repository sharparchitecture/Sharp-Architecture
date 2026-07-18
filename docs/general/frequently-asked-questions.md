# Frequently Asked Questions

**Q: The `Id` property has a protected setter, so it isn't included when the entity is
serialized. How do I include it?**

A: Having the `Id` setter be `protected` is a core `Entity<TId>` design choice — it
keeps application code from assigning ids directly. If you need `Id` included in a
serialized payload (e.g. XML or a DTO), expose it through a read-only wrapper property
or, more commonly today, map the entity to an explicit DTO/view model for
serialization rather than serializing the entity itself:

```csharp
public class AnnouncementModel
{
    public int Id { get; init; }
    public string Title { get; init; } = null!;
}
```

This is also what the [CRUD tutorial](../getting-started/simple-crud-application.md)
does via AutoMapper (`_mapper.Map<AnnouncementModel>(announcement)`), rather than
returning the entity from a controller action directly.

**Q: How do I opt out of Fluent NHibernate auto-mapping for an entity?**

A: `AddMappingAssemblies(...)` on `NHibernateSessionFactoryBuilder` picks up both Fluent
NHibernate `ClassMap<T>`/`IAutoMappingOverride<T>` mappings and NHibernate HBM XML
mappings from the given assemblies, in addition to the auto-persistence model passed to
`UseAutoPersistenceModel(...)`. To fully hand-map a specific entity, add an explicit
`ClassMap<T>` for it and exclude the type from the auto-persistence model
(`AutoPersistenceModel.Where(...)`/`.IgnoreBase<T>()`, depending on your FluentNHibernate
version) — or, for smaller tweaks, use an `IAutoMappingOverride<T>` as shown in the
[CRUD tutorial](../getting-started/simple-crud-application.md) instead of opting out
entirely.

**Q: Can I use S#arp Architecture with more than one database in the same app?**

A: Not out of the box — see [Multiple Databases](../nhibernate/multiple-dbs.md).

**Q: I need faster id generation than one round-trip per insert. What are my options?**

A: See the [HiLo id generator](../nhibernate/hilo-generator.md) page.

**Q: Where did RavenDB / WCF / Templify go?**

A: `SharpArch.RavenDB` was discontinued in 9.0.0 — NHibernate is now the only supported
persistence provider. WCF and Templify were tied to the pre-ASP.NET-Core, .NET Framework
era of the framework and no longer apply; see the [Changelog](../changelog.md) for the
full history.
