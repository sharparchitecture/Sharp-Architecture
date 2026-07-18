# Installation

## Install the NuGet packages

For an ASP.NET Core app using NHibernate:

```bash
dotnet add package SharpArch.Domain
dotnet add package SharpArch.NHibernate
dotnet add package SharpArch.NHibernate.DependencyInjection
dotnet add package SharpArch.Web.AspNetCore
```

| Package | Purpose |
|---|---|
| [`SharpArch.Domain`](https://www.nuget.org/packages/SharpArch.Domain/) | Core interfaces/classes (`Entity<TId>`, `IRepository`, `ILinqRepository`, `ITransactionManager`). Persistence-ignorant. |
| [`SharpArch.NHibernate`](https://www.nuget.org/packages/SharpArch.NHibernate/) | NHibernate session management and the `IRepository`/`ILinqRepository` implementations. |
| [`SharpArch.NHibernate.DependencyInjection`](https://www.nuget.org/packages/SharpArch.NHibernate.DependencyInjection/) | `AddNHibernateWithSingleDatabase(...)` registration helper for `IServiceCollection`. |
| [`SharpArch.Web.AspNetCore`](https://www.nuget.org/packages/SharpArch.Web.AspNetCore/) | `[Transaction]` attribute and `AutoTransactionHandler` MVC filter. |

For your test project, add one of the testing packages — **xUnit is the recommended
framework**; the NUnit package is kept only for existing projects:

```bash
dotnet add package SharpArch.Testing.Xunit.NHibernate
```

## Wire up NHibernate and ASP.NET Core

In `Program.cs`/`Startup.cs`, register the session factory and the transaction filter.
This is the actual wiring used by the [`TardisBank`](../../Src/Samples/TardisBank)
sample
(`Src/Samples/TardisBank/Src/Suteki.TardisBank.WebApi/Startup.cs`):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers(options =>
    {
        options.Filters.Add(new AutoTransactionHandler());
    });

    services.AddNHibernateWithSingleDatabase(_ =>
        new NHibernateSessionFactoryBuilder()
            .AddMappingAssemblies(new[] { typeof(Child).Assembly })
            .UseAutoPersistenceModel(new AutoPersistenceModelGenerator().Generate())
            .UseConfigFile("NHibernate.config"));

    // Repository registration is separate — see below.
}
```

`AddNHibernateWithSingleDatabase` registers:

| Type | Lifetime |
|---|---|
| `ISessionFactory` | Singleton |
| `ISession` | Scoped (per HTTP request) |
| `IStatelessSession` | Scoped |
| `TransactionManager` / `ITransactionManager` / `INHibernateTransactionManager` | Scoped |

> Repository registration is **not** done by `AddNHibernateWithSingleDatabase` — you
> need to register `IRepository<,>`/`ILinqRepository<,>` yourself. The sample above uses
> Autofac (`builder.RegisterGeneric(typeof(LinqRepository<,>)).AsImplementedInterfaces()`),
> but with the built-in container it's just:
>
> ```csharp
> services.AddScoped(typeof(IRepository<,>), typeof(NHibernateRepository<,>));
> services.AddScoped(typeof(ILinqRepository<,>), typeof(LinqRepository<,>));
> ```

`NHibernateSessionFactoryBuilder` is a fluent, transient builder — see its full method
list (mapping assemblies, auto-persistence model, config file, 2nd-level cache,
data-annotation validators, raw `Configuration` escape hatch) in the
[NHibernate](../nhibernate/nhibernate.md) page.

## Only one database at a time

The DI helper only supports a single database per application — see
[Multiple Databases](../nhibernate/multiple-dbs.md) if you need more than one.
