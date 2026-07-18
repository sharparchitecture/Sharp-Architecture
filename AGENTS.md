# AGENTS.md

This file provides guidance to AI coding agents working with code in this repository.

## Project Settings

- **Stack:** dotnet

## What this is

S#arp Architecture is a framework (a set of NuGet libraries, not an application) for building
maintainable ASP.NET Core web applications using Domain-Driven Design on top of NHibernate.
The shipped product is the set of `SharpArch.*` packages under `Src/Lib/`; everything else
(`Src/Tests/`, `Src/Samples/`) exists to test and demonstrate them.

**Ignore the `Old/` folder** — it holds legacy (pre-v5, ASP.NET MVC / Castle Windsor) samples kept
for reference only. Do not modify, build, or use it as a pattern for new work.

## Commands

All commands run from the repository root unless noted. There is a single solution: `Src/SharpArch.sln`.

```bash
# Restore + build (Debug or Release)
cd Src && dotnet build -c Release

# Run the full unit-test suite (excludes DB integration & functional tests)
cd Src && dotnet test -c Release --filter "Category!=IntegrationTests&Category!=Functional"

# Run a single test project
dotnet test Src/Tests/SharpArch.XunitTests/SharpArch.XunitTests.csproj

# Run a single test by name (substring match on fully-qualified name)
dotnet test Src/SharpArch.sln --filter "FullyQualifiedName~EntityTests"

# Full CI pipeline locally (build + tests + coverage + pack) via Cake
dotnet tool install Cake.Tool --global   # once
dotnet cake                              # runs the "Default" target from build.cake
dotnet cake --target=RunUnitTests        # build + tests + coverage only
```

### Test categories & the database

- Unit tests use an **in-memory SQLite** database and need no setup.
- Tests tagged `[Trait("Category","IntegrationTests")]` / `Functional` / `ManualTests` hit a **real SQL
  Server** and are excluded from the default run. To run them locally, start SQL Server first:
  `pwsh Docker/start-mssql.ps1` (SQL Server 2019 container on port **2433**, sa password `Password12!`).

## Target frameworks

Defined centrally in `Src/Directory.Build.props`:
- **Libraries** (`Src/Lib/`): `netstandard2.1;net8.0;net9.0`
- **Apps & test projects**: `net8.0;net9.0`

## Architecture

Strict DDD layering enforced by project references — the dependency direction is the point of the framework.

- **`SharpArch.Domain`** — the core, with **no infrastructure dependencies**. Contains the domain
  building blocks and, crucially, the *persistence-support interfaces* (`IRepository<,>`,
  `ILinqRepository<,>`, `ITransactionManager`, `IEntityDuplicateChecker`) that the domain depends on
  but does not implement. This inversion lets domain and application code stay ORM-agnostic.
  - `Entity<TId>` / `IEntity` — identity-based equality. Two entities are equal if they share a
    non-default `Id`; transient entities fall back to comparing *domain signature* properties.
  - `[DomainSignature]` — marks the properties that define business identity; consumed by
    `Entity.GetHashCode`/`Equals` and by `HasUniqueDomainSignatureAttribute` validation.
  - `ValueObject`, `BaseObject`, `ValidatableObject` — value-equality and validation base types.
  - `Specifications/` — `ILinqSpecification<T>` / `QuerySpecification<T>` (Specification pattern) used
    by the LINQ repository.
- **`SharpArch.NHibernate`** — the NHibernate implementation of the domain's persistence interfaces:
  `NHibernateRepository`/`LinqRepository`, `TransactionManager`, and `NHibernateSessionFactoryBuilder`
  (fluent builder for `ISessionFactory`, supporting external `hibernate.cfg.xml`, Fluent
  auto-persistence models, 2nd-level cache, and data-annotation validators).
- **`SharpArch.NHibernate.DependencyInjection`** — `AddNHibernateWithSingleDatabase(...)` wires the
  above into `IServiceCollection`. Lifetimes matter: **`ISessionFactory` = Singleton**,
  **`ISession` = Scoped** (per HTTP request), **`IStatelessSession` = Transient**, `TransactionManager`
  = Scoped.
- **`SharpArch.Web.AspNetCore`** — MVC integration. `[Transaction]` is a *marker* attribute
  (class/method/global); it does nothing unless `AutoTransactionHandler` is registered as an MVC
  filter. The handler opens a transaction per action and commits/rolls back based on the outcome
  (rollback on unhandled exception, and optionally on model-validation errors).
- **`SharpArch.Infrastructure`** — cross-cutting helpers (`CodeBaseLocator`, logging wrappers).
- **`SharpArch.Testing[.Xunit|.NUnit][.NHibernate]`** — base classes for consumers' tests, e.g.
  `RepositoryTestsBase` / `TransientDatabaseTests` that spin up a throwaway DB per test.

Typical consumer flow: define entities in a Domain layer → depend only on `IRepository`/`ILinqRepository`
→ compose an ASP.NET Core app that calls `AddNHibernateWithSingleDatabase` and registers
`AutoTransactionHandler`. The `Src/Samples/TardisBank` project is the end-to-end reference.


## Conventions

- **File-scoped namespaces**, with `using` directives placed **inside** the namespace block.
- `Nullable` and `ImplicitUsings` are **enabled**; `LangVersion` is `preview`. Global usings include
  `JetBrains.Annotations` and `System.Diagnostics.CodeAnalysis`.
- **Public API is annotated**: `[PublicAPI]` marks exported surface, and `GenerateDocumentationFile`
  is on — public members need XML doc comments (missing-doc warning 1591 is *not* suppressed for
  libraries).
- **Test frameworks**: xUnit is primary (`Shouldly` for assertions, `Moq` for mocking). One legacy
  project, `SharpArch.Tests.NHibernate`, still uses NUnit — the framework is selected by project name
  in `Src/Tests/Directory.Build.props`, so match the surrounding project when adding tests.
- **Versioning/branching**: GitFlow (`develop` = pre-release, `master` = stable) with GitVersion;
  package version is derived from git, not hardcoded.

## Code exploration

A CodeGraph index (`.codegraph/`) is present — prefer `codegraph_explore` for structural questions
(who calls what, where a symbol is defined, blast radius) over grep/read loops. See the global
guidance for details.

