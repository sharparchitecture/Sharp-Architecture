# Multiple Databases

The built-in dependency-injection helper,
[`AddNHibernateWithSingleDatabase`](../../Src/Lib/SharpArch.NHibernate.DependencyInjection/NHibernateRegistrationExtensions.cs),
registers exactly one `ISessionFactory`/`ISession`/`IStatelessSession`/`ITransactionManager`
set per application — as the name says, it targets the single-database case, and there
is currently no first-class, out-of-the-box registration helper for multiple databases
in one process.

> The codebase still carries an `ISessionFactoryKeyProvider` interface from an earlier
> multi-database design, but nothing in `SharpArch.NHibernate` implements or consumes it
> today — treat it as a leftover extension point, not a supported feature.

## If you need more than one database

You can still wire up multiple NHibernate session factories yourself, on top of
`NHibernateSessionFactoryBuilder`, using ASP.NET Core's
[keyed services](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#keyed-services)
(available since .NET 8):

```csharp
services.AddKeyedSingleton("Northwind", (sp, _) =>
    new NHibernateSessionFactoryBuilder()
        .AddMappingAssemblies(new[] { typeof(Northwind.Infrastructure.AssemblyMarker).Assembly })
        .UseConfigFile("NorthwindNHibernate.config")
        .BuildSessionFactory());

services.AddKeyedSingleton("Village", (sp, _) =>
    new NHibernateSessionFactoryBuilder()
        .AddMappingAssemblies(new[] { typeof(Village.Infrastructure.AssemblyMarker).Assembly })
        .UseConfigFile("VillageNHibernate.config")
        .BuildSessionFactory());
```

Then resolve `ISessionFactory` per database with
`[FromKeyedServices("Northwind")]`, open your own `ISession`/`IStatelessSession` scoped
per request from each keyed factory, and register a repository implementation per
database (a repository class per `(entity, database)` pair, since `NHibernateRepository`
takes a single `ISession` in its constructor).

This is considerably more manual than the single-database path — budget for writing your
own scoped-session and transaction-manager wiring for the second (and subsequent)
databases.
