# Architecture

S#arp Architecture enforces a strict Domain-Driven Design layering, using project
references to make the dependency direction hard to violate by accident:

- **Domain**
- **Infrastructure**
- **Presentation**
- **Tests**

## Domain

The *Domain* layer is where business entities and business logic live. It should stay
persistence-ignorant: `SharpArch.Domain` has no infrastructure dependencies of its own.
It does, however, define the *persistence-support interfaces* — `IRepository<TEntity,TId>`,
`ILinqRepository<TEntity,TId>`, `ITransactionManager`, `IEntityDuplicateChecker` — that
the domain depends on but does not implement. This inversion is what keeps domain and
application code ORM-agnostic.

Building blocks provided here:

- `Entity<TId>` / `IEntity` — identity-based equality. Two entities are equal if they
  share a non-default `Id`; transient entities (no `Id` yet) fall back to comparing
  their *domain signature* properties.
- `[DomainSignature]` — marks the properties that define an entity's business identity;
  used by `Entity.GetHashCode()`/`Equals()` and by domain-signature-uniqueness
  validation.
- `ValueObject`, `BaseObject`, `ValidatableObject` — value-equality and validation base
  types.
- `Specifications/` — `ILinqSpecification<T>` / `QuerySpecification<T>`, an
  implementation of the Specification pattern used by the LINQ repository.

Additional occupants that typically live alongside the domain layer:

- Contracts (interfaces) for application/task services
- Contracts for query objects
- Domain events

> Prior to version 4, S#arp Architecture shipped a dedicated *Tasks* layer for
> non-persistence application services. As of v4 there is no built-in Tasks layer —
> use a library such as [MediatR](https://github.com/jbogard/MediatR) for commands,
> queries, and event/command handlers instead.

## Infrastructure

The *Infrastructure* layer wires up NHibernate: session factory configuration, Fluent
NHibernate mappings, and dependency injection registration
(`SharpArch.NHibernate`, `SharpArch.NHibernate.DependencyInjection`). You can extend the
provided repository implementations with additional query methods, but it's recommended
to write your own query/specification objects instead of growing a generic repository
without bound.

## Presentation

The *Presentation* layer is an ASP.NET Core project — MVC, Web API, or Blazor —
containing controllers/endpoints, view models, and application startup/composition
(`SharpArch.Web.AspNetCore` provides the `[Transaction]` attribute and
`AutoTransactionHandler` MVC filter described in [Unit of Work](unit-of-work.md)).

Additional occupants:

- View models (can live in Presentation, or alongside the domain if a service layer
  returns them directly — uncommon, since view models are usually tied to a specific
  view)
- Query objects (Presentation or application-service layer, depending on where the
  query is needed)
- Controllers / minimal API endpoints
- Views

## Tests

`SharpArch.Testing`, `SharpArch.Testing.Xunit[.NHibernate]`, and
`SharpArch.Testing.NUnit` provide base classes — e.g. `RepositoryTestsBase` /
`TransientDatabaseTests` — that spin up a throwaway database per test, so repository and
mapping tests don't need a shared test database.

See [`Src/Samples/TardisBank`](../Src/Samples/TardisBank) for an end-to-end reference
that ties all four layers together.
