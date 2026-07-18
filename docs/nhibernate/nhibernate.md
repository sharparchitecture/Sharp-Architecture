# NHibernate

NHibernate is S#arp Architecture's only supported persistence provider as of 9.0.0 (the
`SharpArch.RavenDB` library was discontinued in that release). The `SharpArch.NHibernate`
and `SharpArch.NHibernate.DependencyInjection` packages provide:

- `NHibernateSessionFactoryBuilder` — a fluent builder for `ISessionFactory`
  (mapping assemblies, Fluent auto-persistence models, `hibernate.cfg.xml`, 2nd-level
  cache, data-annotation validators).
- `AddNHibernateWithSingleDatabase(...)` — wires the builder's output into
  `IServiceCollection` with the right lifetimes (see [Installation](../getting-started/installation.md)).
- `NHibernateRepository<TEntity,TId>` / `LinqRepository<TEntity,TId>` — implementations
  of the domain's `IRepository`/`ILinqRepository` interfaces.
- `TransactionManager` — the NHibernate implementation of `ITransactionManager`, used
  by `[Transaction]` / `AutoTransactionHandler` (see [Unit of Work](../general/unit-of-work.md)).

## Topics

- [HiLo id generator](hilo-generator.md)
- [Multiple databases](multiple-dbs.md)

For a complete, working example of all of the above, see the
[`TardisBank`](../../Src/Samples/TardisBank) sample project.
