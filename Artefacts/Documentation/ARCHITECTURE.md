# Sharp-Architecture — Component Architecture

> Generated from source analysis of `Src/Lib` and `Src/Samples`.
> Last updated: June 2026

---

## Table of Contents

- [Sharp-Architecture — Component Architecture](#sharp-architecture--component-architecture)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Package Structure](#package-structure)
  - [Domain Model Layer](#domain-model-layer)
    - [Class Hierarchy](#class-hierarchy)
    - [Key Interfaces — Domain Model](#key-interfaces--domain-model)
    - [Domain Signature](#domain-signature)
    - [Equality \& Comparison Utilities](#equality--comparison-utilities)
    - [Reflection Cache](#reflection-cache)
  - [Persistence Abstractions](#persistence-abstractions)
    - [Repository Contracts](#repository-contracts)
    - [Transaction Contracts](#transaction-contracts)
    - [Duplicate Detection Contract](#duplicate-detection-contract)
    - [Validation Attributes](#validation-attributes)
      - [`[HasUniqueDomainSignature]`](#hasuniquedomainsignature)
    - [Specification Pattern](#specification-pattern)
    - [Repository Extensions](#repository-extensions)
  - [NHibernate Implementations](#nhibernate-implementations)
    - [Interface Inheritance](#interface-inheritance)
    - [Class Implementations](#class-implementations)
      - [`TransactionManager`](#transactionmanager)
      - [`NHibernateRepository<TEntity, TId>`](#nhibernaterepositorytentity-tid)
      - [`LinqRepository<TEntity, TId>`](#linqrepositorytentity-tid)
      - [`EntityDuplicateChecker`](#entityduplicatechecker)
    - [Session \& Query Utilities](#session--query-utilities)
      - [`Enums.LockMode`](#enumslockmode)
    - [Session Factory Setup](#session-factory-setup)
    - [FluentNHibernate Integration](#fluentnhibernate-integration)
    - [Data Annotation Validation Hook](#data-annotation-validation-hook)
  - [Infrastructure Utilities](#infrastructure-utilities)
    - [`LogWrapper` (struct, in `SharpArch.Infrastructure.Logging`)](#logwrapper-struct-in-sharparchinfrastructurelogging)
    - [`CodeBaseLocator`](#codebaselocator)
  - [ASP.NET Core Web Layer](#aspnet-core-web-layer)
    - [Declarative Transaction Management](#declarative-transaction-management)
    - [Filter Pipeline](#filter-pipeline)
  - [Dependency Injection Wiring](#dependency-injection-wiring)
  - [End-to-End Request Flow](#end-to-end-request-flow)
  - [Testing Infrastructure](#testing-infrastructure)
    - [Base Test Classes (xUnit)](#base-test-classes-xunit)
      - [`TransientDatabaseTests<TDatabaseInitializer>` (`SharpArch.Testing.Xunit.NHibernate`)](#transientdatabaseteststdatabaseinitializer-sharparchtestingxunitnhibernate)
      - [`LiveDatabaseTests<TDatabaseSetup>` (`SharpArch.Testing.Xunit.NHibernate`)](#livedatabaseteststdatabasesetup-sharparchtestingxunitnhibernate)
    - [Base Test Classes (NUnit)](#base-test-classes-nunit)
    - [General Test Helpers](#general-test-helpers)
      - [`SetCultureAttribute` (`SharpArch.Testing.Xunit`)](#setcultureattribute-sharparchtestingxunit)
  - [Sample Application — TardisBank](#sample-application--tardisbank)
    - [Project Structure](#project-structure)
    - [Layer → Framework Dependency Mapping](#layer--framework-dependency-mapping)
  - [Component Dependency Graph](#component-dependency-graph)
  - [NuGet Package Dependencies](#nuget-package-dependencies)
    - [`SharpArch.Domain.csproj`](#sharparchdomaincsproj)
    - [`SharpArch.NHibernate.csproj`](#sharparchnhibernatecsproj)
    - [`SharpArch.NHibernate.Extensions.DependencyInjection.csproj`](#sharparchnhibernateextensionsdependencyinjectioncsproj)
    - [`SharpArch.Web.AspNetCore.csproj`](#sharparchwebaspnetcorecsproj)

---

## Overview

Sharp-Architecture is a .NET framework built around **Domain-Driven Design (DDD)** principles. It provides:

- A clean **domain model base** (entities, value objects, specifications) with **zero external dependencies**.
- **Repository and Transaction Manager abstractions** defined in the domain layer and implemented against NHibernate.
- **ASP.NET Core MVC filter** support for declarative, attribute-driven transaction management.
- **Dependency Injection extensions** to wire everything together with minimal boilerplate.
- **Testing base classes** for both xUnit and NUnit, supporting in-memory and live-database scenarios.

The architecture enforces a strict **dependency direction**: only outer layers depend on inner layers. The domain layer has no knowledge of any ORM, web framework, or infrastructure library.

```
┌────────────────────────────────────────────────────────────────┐
│                     Web / Application                          │
│   SharpArch.Web.AspNetCore                                     │
├────────────────────────────────────────────────────────────────┤
│                 ORM / Infrastructure                           │
│   SharpArch.NHibernate                                         │
│   SharpArch.NHibernate.DependencyInjection                     │
├────────────────────────────────────────────────────────────────┤
│                  Domain (Core — no external deps)              │
│   SharpArch.Domain                                             │
├────────────────────────────────────────────────────────────────┤
│               Cross-cutting Utilities                          │
│   SharpArch.Infrastructure                                     │
└────────────────────────────────────────────────────────────────┘
```

---

## Package Structure

| Package | Responsibility |
|---|---|
| `SharpArch.Domain` | Core DDD building blocks: entities, value objects, repository contracts, specifications, validation. **Zero external dependencies.** |
| `SharpArch.Infrastructure` | Cross-cutting utilities: allocation-free logging wrapper (`LogWrapper`), code-base locator. Depends only on `Microsoft.Extensions.Logging`. |
| `SharpArch.NHibernate` | NHibernate implementations of domain repository/transaction contracts. Includes FluentNHibernate automapping support and Data Annotation validation hooks. |
| `SharpArch.NHibernate.DependencyInjection` | ASP.NET Core `IServiceCollection` extension `AddNHibernateWithSingleDatabase` that registers session factory, sessions, and transaction manager with correct lifetimes. |
| `SharpArch.Web.AspNetCore` | `[Transaction]` attribute and `AutoTransactionHandler` MVC action filter for declarative transaction management. |
| `SharpArch.Testing` | Framework-agnostic test base types and helper utilities shared between NUnit and xUnit packages. |
| `SharpArch.Testing.NUnit` | NUnit base classes: `RepositoryTestsBase` (transient DB) and `DatabaseRepositoryTestsBase` (live DB). |
| `SharpArch.Testing.Xunit` | xUnit attribute `SetCultureAttribute` for per-test/per-class culture control. |
| `SharpArch.Testing.Xunit.NHibernate` | xUnit base classes: `TransientDatabaseTests` (in-memory/rollback) and `LiveDatabaseTests` (live DB, rollback per test). |

---

## Domain Model Layer

### Class Hierarchy

```
System.Object
 └── BaseObject                               (abstract)
      │   ▸ Signature-based Equals / GetHashCode
      │   ▸ HasSameObjectSignatureAs(BaseObject)
      │   ▸ GetTypeUnproxied() — NHibernate proxy-safe type resolution
      │   ▸ Uses static ITypePropertyDescriptorCache to cache PropertyInfo[]
      │
      ├── ValueObject                          (abstract)
      │       ▸ Signature = ALL properties (by convention)
      │       ▸ [DomainSignature] MUST NOT be used on value object properties
      │       ▸ Throws InvalidOperationException if [DomainSignature] detected
      │       ▸ == / != operators
      │
      └── ValidatableObject                    (abstract)
               ▸ DataAnnotations validation via Validator.TryValidateObject
               ▸ IsValid(ValidationContext) → bool
               ▸ ValidationResults(ValidationContext) → ICollection<ValidationResult>
               │
               └── Entity<TId>                (abstract, generic)
                       where TId : IEquatable<TId>
                       ▸ Id : TId  (virtual, protected setter, [XmlIgnore])
                       ▸ IsTransient() — true when Id is null or equals default(TId)
                       ▸ GetId() — untyped object? accessor (avoids boxing where possible)
                       ▸ GetTypeUnproxied() — virtual, overridable for proxy detection
                       ▸ IEquatable<Entity<TId>>, == / != operators
                       ▸ Implements IEntity<TId> and IEntity
                       ▸ Hash code cached after first computation
```

### Key Interfaces — Domain Model

| Interface | Purpose |
|---|---|
| `IEntity` | Non-generic entity contract: `GetId()`, `GetSignatureProperties()`, `IsTransient()`, `GetTypeUnproxied()` |
| `IEntity<TId>` | Generic entity contract: typed `Id` property (covariant `out TId`) |
| `IHasAssignedId<TId>` | Marker for entities with **manually assigned** IDs. Use when IDs are not auto-generated by the database (e.g., natural keys). `SetAssignedIdTo(TId)` must be implemented. |

### Domain Signature

`[DomainSignature]` is a property-level attribute that marks properties forming the **business identity** of an entity — the set of properties that determine whether two instances represent the same real-world concept (e.g., a `User`'s `Email`, a `Product`'s `Sku`).

**How it works:**

1. `BaseObject.GetSignatureProperties()` uses reflection to find all properties decorated with `[DomainSignature]` on the concrete type.
2. The result is cached in `TypePropertyDescriptorCache` (static, thread-safe) to avoid repeated reflection.
3. `HasSameObjectSignatureAs(BaseObject)` compares all signature property values.
4. `Entity<TId>.Equals()` first tries ID comparison, then falls back to signature comparison for transient objects.
5. `EntityDuplicateChecker.DoesDuplicateExistWithTypedIdOf(IEntity)` queries the database for entities with matching signature property values.

```csharp
// Example usage:
public class User : Entity<int>
{
    [DomainSignature]
    public virtual string Email { get; set; }

    public virtual string Name { get; set; }  // Not part of domain signature
}
```

**Rules:**
- `ValueObject` subclasses use **all** properties as signature — `[DomainSignature]` must not be applied.
- `Entity<TId>` subclasses should have at least one `[DomainSignature]` property.

### Equality & Comparison Utilities

| Type | Purpose |
|---|---|
| `BaseObjectEqualityComparer<T>` | Implements `IEqualityComparer<T>` for use with LINQ set operators (`Intersect`, `Union`, `Distinct`). Delegates to `BaseObject.Equals` and `GetHashCode`. Necessary because LINQ set operators use `IEqualityComparer<T>.GetHashCode` rather than `object.GetHashCode`. |
| `DomainSignatureAttribute` | Attribute applied to entity properties to include them in domain signature comparison. |

```csharp
// Example: using BaseObjectEqualityComparer with LINQ
var distinct = users.Distinct(new BaseObjectEqualityComparer<User>());
var intersection = listA.Intersect(listB, new BaseObjectEqualityComparer<User>());
```

### Reflection Cache

```
ITypePropertyDescriptorCache
 └── TypePropertyDescriptorCache              (thread-safe, static instance in BaseObject)
          └── TypePropertyDescriptor          (wraps Type + PropertyInfo[])
```

- `TypePropertyDescriptorCache` is a concurrent dictionary keyed by `Type`.
- `BaseObject` holds a single static instance shared across all object instances.
- `GetOrAdd(type, factory)` is used to prevent redundant allocations on the first access.

---

## Persistence Abstractions

All persistence contracts live in `SharpArch.Domain.PersistenceSupport`. They carry **no ORM dependency** — they reference only `SharpArch.Domain.DomainModel` types.

### Repository Contracts

```
IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : IEquatable<TId>
 │
 ├── TransactionManager : ITransactionManager
 ├── GetAsync(TId, CancellationToken) → Task<TEntity?>
 ├── GetAllAsync(CancellationToken) → Task<IList<TEntity>>
 ├── SaveAsync(entity, CancellationToken) → Task<TEntity>
 ├── SaveOrUpdateAsync(entity, CancellationToken) → Task<TEntity>
 ├── EvictAsync(entity, CancellationToken) → Task
 ├── DeleteAsync(entity, CancellationToken) → Task
 └── DeleteAsync(TId, CancellationToken) → Task

ILinqRepository<TEntity, TId>   extends   IRepository<TEntity, TId>
 ├── FindOneAsync(TId, CancellationToken) → Task<TEntity?>
 ├── FindOneAsync(ILinqSpecification<TEntity>, CancellationToken) → Task<TEntity?>
 ├── FindAll() → IQueryable<TEntity>
 └── FindAll(ILinqSpecification<TEntity>) → IQueryable<TEntity>
```

### Transaction Contracts

```
ITransactionManager
 ├── BeginTransaction(IsolationLevel) → IDisposable
 ├── CommitTransactionAsync(CancellationToken) → Task
 └── RollbackTransactionAsync(CancellationToken) → Task

ISupportsTransactionStatus
 └── IsActive : bool
```

`ISupportsTransactionStatus` is an **optional extension interface**. `AutoTransactionHandler` checks whether the resolved `ITransactionManager` also implements this interface, and if so, verifies `IsActive` before attempting commit/rollback. This prevents double-commit errors if the action manually managed the transaction.

### Duplicate Detection Contract

```
IEntityDuplicateChecker
 └── DoesDuplicateExistWithTypedIdOf(IEntity) : bool
```

Implemented by `EntityDuplicateChecker` in `SharpArch.NHibernate`. Used internally by the `[HasUniqueDomainSignature]` validator attribute.

### Validation Attributes

The `SharpArch.Domain.Validation` namespace provides Data Annotation validators that integrate with the entity lifecycle:

#### `[HasUniqueDomainSignature]`

Applied to an entity **class** to validate that no other persisted entity has the same domain signature.

```
HasUniqueDomainSignatureAttributeBase   (abstract base)
 └── HasUniqueDomainSignatureAttribute  (concrete, resolves IEntityDuplicateChecker from ValidationContext)
```

**Flow:**
1. Applied to an entity class: `[HasUniqueDomainSignature]`
2. When `entity.IsValid(validationContext)` or `entity.ValidationResults(...)` is called:
3. `HasUniqueDomainSignatureAttribute.IsValid(entity, context)` is invoked.
4. It retrieves `IEntityDuplicateChecker` from `validationContext.GetService<IEntityDuplicateChecker>()`.
5. Calls `IEntityDuplicateChecker.DoesDuplicateExistWithTypedIdOf(entity)`.
6. Returns a validation error if a duplicate exists.

```csharp
[HasUniqueDomainSignature]
public class User : Entity<int>
{
    [DomainSignature]
    public virtual string Email { get; set; }
}
```

### Specification Pattern

Specifications encapsulate and name query logic, enabling reuse and composition.

```
ISpecification<T>
 └── IsSatisfiedBy(T) : bool             ← in-memory evaluation

ILinqSpecification<T>
 └── SatisfyingElementsFrom(IQueryable<T>) → IQueryable<T>   ← DB query

QuerySpecification<T>                    (abstract base — implements ILinqSpecification<T>)
 └── override SatisfyingElementsFrom(candidates)

AdHoc<T>                                 (extends QuerySpecification<T>)
 └── Created via lambda: new AdHoc<User>(q => q.Where(u => u.IsActive))
```

`QuerySpecificationExtensions` provides LINQ-style extension methods for composing specifications.

**Usage pattern:**

```csharp
// Named specification
public class ActiveUsersSpec : QuerySpecification<User>
{
    public override IQueryable<User> SatisfyingElementsFrom(IQueryable<User> candidates)
        => candidates.Where(u => u.IsActive);
}

// Inline ad-hoc specification
var spec = new AdHoc<User>(q => q.Where(u => u.Email.EndsWith("@example.com")));

// Usage with repository
IQueryable<User> activeUsers = linqRepo.FindAll(new ActiveUsersSpec());
User? user = await linqRepo.FindOneAsync(spec, cancellationToken);
```

### Repository Extensions

`RepositoryExtensions` provides convenience extension methods on `IRepository<TEntity, TId>`:

- Helpers for common patterns such as checking existence or combining operations.
- Reduces boilerplate in application code that uses `IRepository`.

---

## NHibernate Implementations

### Interface Inheritance

```
Domain Layer                          NHibernate Layer
─────────────────                     ────────────────────────────────────────
ITransactionManager           ◄───── INHibernateTransactionManager
                                        └── + Session : ISession
                                        └── + FlushChangesAsync()

IRepository<TEntity,TId>      ◄───── INHibernateRepository<TEntity,TId>
                                        └── + FindAllAsync(propertyValuePairs)
                                        └── + FindAllAsync(exampleInstance)
                                        └── + FindOneAsync(exampleInstance)
                                        └── + FindOneAsync(propertyValuePairs)
                                        └── + GetAsync(id, lockMode)
                                        └── + LoadAsync(id)
                                        └── + LoadAsync(id, lockMode)
                                        └── + MergeAsync(entity)
                                        └── + UpdateAsync(entity)

ILinqRepository<TEntity,TId>  ◄───── LinqRepository<TEntity,TId>  (class)
```

### Class Implementations

#### `TransactionManager`

```
TransactionManager
 ├── implements  INHibernateTransactionManager
 ├── implements  ISupportsTransactionStatus
 ├── constructor: TransactionManager(ISession session)
 ├── Session : ISession                            → exposes raw NHibernate session
 ├── BeginTransaction(IsolationLevel)              → Session.BeginTransaction(...)
 ├── CommitTransactionAsync(ct)                    → Session.GetCurrentTransaction().CommitAsync(ct)
 ├── RollbackTransactionAsync(ct)                  → Session.GetCurrentTransaction().RollbackAsync(ct)
 ├── FlushChangesAsync(ct)                         → Session.FlushAsync(ct)
 └── IsActive                                      → Session.GetCurrentTransaction()?.IsActive ?? false
```

#### `NHibernateRepository<TEntity, TId>`

```
NHibernateRepository<TEntity, TId>
 ├── implements  INHibernateRepository<TEntity, TId>
 ├── constructor: NHibernateRepository(INHibernateTransactionManager transactionManager)
 ├── Session : ISession                            → TransactionManager.Session
 ├── GetAsync(id, ct)                              → Session.GetAsync<TEntity>(id, ct)
 ├── GetAllAsync(ct)                               → Session.CreateCriteria(...).ListAsync(ct)
 ├── SaveAsync(entity, ct)                         → Session.SaveAsync(entity, ct)
 ├── SaveOrUpdateAsync(entity, ct)                 → Session.SaveOrUpdateAsync(entity, ct)
 ├── EvictAsync(entity, ct)                        → Session.EvictAsync(entity, ct)
 ├── DeleteAsync(entity, ct)                       → Session.DeleteAsync(entity, ct)
 ├── DeleteAsync(id, ct)                           → GetAsync then DeleteAsync
 ├── FindAllAsync(propertyValuePairs, max, ct)     → Criteria with Restrictions.Eq / IsNull
 ├── FindAllAsync(example, exclusions, max, ct)    → Criteria with Example.Create(...)
 ├── FindOneAsync(example, ct, exclusions)         → FindAllAsync (maxResults=2) + uniqueness check
 ├── FindOneAsync(propertyValuePairs, ct)          → FindAllAsync (maxResults=2) + uniqueness check
 ├── GetAsync(id, lockMode, ct)                    → Session.GetAsync<TEntity>(id, lockMode, ct)
 ├── LoadAsync(id, ct)                             → Session.LoadAsync<TEntity>(id, ct)
 ├── LoadAsync(id, lockMode, ct)                   → Session.LoadAsync<TEntity>(id, lockMode, ct)
 ├── MergeAsync(entity, ct)                        → Session.MergeAsync(entity, ct)
 └── UpdateAsync(entity, ct)                       → Session.UpdateAsync(entity, ct)
```

#### `LinqRepository<TEntity, TId>`

```
LinqRepository<TEntity, TId>
 ├── extends  NHibernateRepository<TEntity, TId>
 ├── implements  ILinqRepository<TEntity, TId>
 ├── FindOneAsync(id, ct)                          → Session.GetAsync<TEntity>(id, ct)
 ├── FindOneAsync(ILinqSpecification, ct)          → spec.SatisfyingElementsFrom(Session.Query<T>()).SingleOrDefaultAsync()
 ├── FindAll()                                     → Session.Query<TEntity>()
 └── FindAll(ILinqSpecification)                   → spec.SatisfyingElementsFrom(Session.Query<T>())
```

#### `EntityDuplicateChecker`

```
EntityDuplicateChecker
 ├── implements  IEntityDuplicateChecker
 ├── constructor: EntityDuplicateChecker(ISession session)
 ├── Temporarily sets FlushMode to Manual during check (avoids flushing pending changes)
 ├── Builds ICriteria excluding the current entity's ID
 ├── Appends criteria for each [DomainSignature] property:
 │    ├── IEntity<> properties    → compared by .id
 │    ├── ValueObject properties  → recursively expanded to scalar comparisons
 │    ├── DateTime properties     → Eq or IsNull (skips uninitialized dates)
 │    ├── string properties       → InsensitiveLike (case-insensitive exact match)
 │    ├── enum properties         → Eq (as int)
 │    └── value types             → Eq or IsNull
 └── Returns true if at least one duplicate found (SetMaxResults(1))
```

### Session & Query Utilities

| Type | Purpose |
|---|---|
| `NHibernateQuery` | Base class / helpers for constructing HQL or Criteria queries; provides common query patterns. |
| `SessionExtensions` | Extension methods on `ISession`, e.g., helpers for common session operations. |
| `LockModeConvertExtensions` | Converts `SharpArch.Domain.Enums.LockMode` to the NHibernate `LockMode` enum, keeping the domain layer free of NHibernate references. |
| `ISessionFactoryKeyProvider` | Contract for providing a named key for multi-database session factory scenarios. |

#### `Enums.LockMode`

Defined in `SharpArch.Domain` (no NHibernate dependency), maps to NHibernate lock modes via `LockModeConvertExtensions`:

| SharpArch LockMode | NHibernate LockMode |
|---|---|
| `None` | `LockMode.None` |
| `Read` | `LockMode.Read` |
| `Upgrade` | `LockMode.Upgrade` |
| `UpgradeNoWait` | `LockMode.UpgradeNoWait` |
| `Force` | `LockMode.Force` |
| `Write` | `LockMode.Write` |

### Session Factory Setup

`NHibernateSessionFactoryBuilder` is a **fluent builder** (instantiated as transient) that produces an `ISessionFactory` (registered as singleton in DI).

```
NHibernateSessionFactoryBuilder
 │
 ├── UseConfigFile(string path)
 │     Loads hibernate.cfg.xml from specified path.
 │     Default: looks for "hibernate.cfg.xml" in working directory.
 │
 ├── UseProperties(IEnumerable<KeyValuePair<string, string>> props)
 │     Sets NHibernate configuration properties programmatically.
 │
 ├── UsePersistenceConfigurer(IPersistenceConfigurer configurer)
 │     FluentNHibernate database configuration
 │     e.g., SQLiteConfiguration.Standard.InMemory()
 │         or MsSqlConfiguration.MsSql2012.ConnectionString(...)
 │
 ├── AddMappingAssemblies(IEnumerable<Assembly> assemblies)
 │     Scans assemblies for:
 │       - HBM XML mappings (AddFromAssembly)
 │       - FluentNHibernate class maps (AddFromAssembly + Conventions)
 │
 ├── UseAutoPersistenceModel(AutoPersistenceModel model)
 │     Adds FluentNHibernate auto-persistence model for convention-based mapping.
 │
 ├── UseCache(Action<CacheSettingsBuilder> cacheConfig)
 │     Configures NHibernate second-level cache.
 │
 ├── UseDataAnnotationValidators(bool enable)
 │     When true, registers DataAnnotationsEventListener as pre-insert
 │     and pre-update event listener on the Configuration.
 │
 ├── ExposeConfiguration(Action<Configuration> config)
 │     Callback invoked after all settings applied; allows direct NHibernate
 │     Configuration manipulation (e.g., schema export, schema update).
 │
 ├── BuildConfiguration() → NHibernate.Cfg.Configuration
 └── BuildSessionFactory() → ISessionFactory
```

### FluentNHibernate Integration

Located in `SharpArch.NHibernate/FluentNHibernate/`:

| Type | Role |
|---|---|
| `IAutoPersistenceModelGenerator` | Contract: implement to provide a customised `AutoPersistenceModel` for auto-mapping. Pass result to `NHibernateSessionFactoryBuilder.UseAutoPersistenceModel(...)`. |
| `IMapGenerator` | Contract: implement to generate `ClassMap<T>` instances for explicit FluentNHibernate mappings. |
| `AutomappingConfiguration` | Base `DefaultAutomappingConfiguration` subclass pre-configured to map only `Entity<TId>` subtypes, ignoring value objects and non-entity types. |
| `FluentNHibernateExtensions` | Extension methods simplifying common FluentNHibernate configuration tasks. |
| `GeneratorHelper` | Helpers for configuring ID generation strategies (identity, assigned, hilo, etc.) in FluentNHibernate convention-based mappings. |
| `Conventions/` | Pre-built FluentNHibernate conventions (e.g., table naming, foreign key naming, cascade defaults). |

### Data Annotation Validation Hook

`DataAnnotationsEventListener` implements NHibernate's `IPreInsertEventListener` and `IPreUpdateEventListener`:

- Invoked automatically by NHibernate **before** every `INSERT` and `UPDATE`.
- Uses `Validator.TryValidateObject` to run all Data Annotation validators on the entity.
- Throws a `ValidationException` if validation fails, preventing the database write.
- Registered via `NHibernateSessionFactoryBuilder.UseDataAnnotationValidators(true)`.

---

## Infrastructure Utilities

### `LogWrapper` (struct, in `SharpArch.Infrastructure.Logging`)

An allocation-free logging helper that skips the `ILogger.Log` call (and avoids lambda captures / string interpolation allocations) when a log level is disabled:

```csharp
public readonly struct LogWrapper
{
    // Returns EnabledLogger? — non-null only when the level is enabled
    public EnabledLogger? Trace    { get; }
    public EnabledLogger? Debug    { get; }
    public EnabledLogger? Information { get; }
    public EnabledLogger? Warning  { get; }
    public EnabledLogger? Error    { get; }
    public EnabledLogger? Critical { get; }
}

// Usage:
var logger = new LogWrapper(loggerFactory.CreateLogger("..."));
logger.Debug?.Log("Session created: {Id}", session.GetSessionImplementation().SessionId);
//            ^^^^ short-circuits if Debug is disabled — no string formatting occurs
```

`EnabledLogger` wraps the actual `ILogger` and exposes a `Log(string message, params object[] args)` method.

### `CodeBaseLocator`

Provides utilities for locating the application code base directory at runtime — useful for finding configuration files (e.g., `hibernate.cfg.xml`) relative to the assembly location rather than the working directory.

---

## ASP.NET Core Web Layer

### Declarative Transaction Management

The `[Transaction]` attribute is a **marker** — it carries configuration but does not apply any behaviour itself. Behaviour is provided by `AutoTransactionHandler`.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TransactionAttribute : Attribute, IFilterMetadata
{
    IsolationLevel IsolationLevel { get; }             // default: ReadCommitted
    bool RollbackOnModelValidationError { get; }       // default: true
}
```

Apply at **controller class** level (all actions use it) or override per **action method**:

```csharp
[Transaction]                          // all actions: ReadCommitted, rollback on validation error
public class OrdersController : ControllerBase
{
    [Transaction(IsolationLevel.Serializable)]   // this action: Serializable
    public Task<IActionResult> Create([FromBody] CreateOrderRequest request) { ... }

    [Transaction(rollbackOnModelValidationError: false)]   // this action: don't rollback on bad model
    public Task<IActionResult> Update(...) { ... }
}
```

### Filter Pipeline

```
ApplyTransactionFilterBase                        (abstract base)
 └── Thread-safe action-descriptor-ID → TransactionAttribute? cache
      Uses copy-on-write dictionary pattern (lock + new dict copy) for thread safety

AutoTransactionHandler
 ├── extends  ApplyTransactionFilterBase
 ├── implements  IAsyncActionFilter
 │
 ├── BEFORE action execution:
 │    1. GetTransactionAttribute(context) — looks up cache, reads effective policy
 │    2. If [Transaction] present:
 │         → context.HttpContext.RequestServices.GetRequiredService<ITransactionManager>()
 │         → transactionManager.BeginTransaction(attribute.IsolationLevel)
 │
 └── AFTER action execution:
      1. If ITransactionManager also implements ISupportsTransactionStatus:
           → if !IsActive: log "Transaction already closed" and return (no double-commit)
      2. if (executedContext.Exception != null
             || attribute.RollbackOnModelValidationError && !context.ModelState.IsValid):
           → RollbackTransactionAsync()    (no cancellation token on error path)
      3. else:
           → CommitTransactionAsync(context.HttpContext.RequestAborted)
```

**Registration:**

```csharp
// In Program.cs / Startup.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AutoTransactionHandler>();
});
```

---

## Dependency Injection Wiring

`NHibernateRegistrationExtensions.AddNHibernateWithSingleDatabase` (in `SharpArch.NHibernate.DependencyInjection`) registers the complete NHibernate infrastructure stack:

| Service | Lifetime | Notes |
|---|---|---|
| `ISessionFactory` | **Singleton** | Built once at startup by `NHibernateSessionFactoryBuilder`. Hash code logged on creation. |
| `ISession` | **Scoped** (per HTTP request) | Opened from `ISessionFactory.OpenSession()` or via optional `sessionConfigurator` callback. Session ID logged on creation. |
| `IStatelessSession` | **Scoped** | Opened from `ISessionFactory.OpenStatelessSession()`. Stateless — no identity map or change tracking. Caller is responsible for disposal. |
| `TransactionManager` | **Scoped** | Wraps the scoped `ISession`. One per request. |
| `INHibernateTransactionManager` | Transient (→ `TransactionManager`) | |
| `ITransactionManager` | Transient (→ `TransactionManager`) | |

**Signature:**

```csharp
public static IServiceCollection AddNHibernateWithSingleDatabase(
    this IServiceCollection services,
    Func<IServiceProvider, NHibernateSessionFactoryBuilder> configureSessionFactory,
    Func<ISessionBuilder, IServiceProvider, ISession>? sessionConfigurator = null,
    Func<IStatelessSessionBuilder, IServiceProvider, IStatelessSession>? statelessSessionConfigurator = null
)
```

**Full setup example:**

```csharp
// Program.cs
builder.Services.AddNHibernateWithSingleDatabase(sp =>
    new NHibernateSessionFactoryBuilder()
        .UsePersistenceConfigurer(
            MsSqlConfiguration.MsSql2012.ConnectionString(
                builder.Configuration.GetConnectionString("Default")))
        .AddMappingAssemblies(new[] { typeof(ProductMap).Assembly })
        .UseDataAnnotationValidators(true)
        .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AutoTransactionHandler>();
});

// Repository registrations (not auto-registered)
builder.Services.AddTransient<IRepository<Product, int>, NHibernateRepository<Product, int>>();
builder.Services.AddTransient<ILinqRepository<Order, int>, LinqRepository<Order, int>>();
builder.Services.AddTransient<IEntityDuplicateChecker, EntityDuplicateChecker>();
```

---

## End-to-End Request Flow

```
HTTP Request
     │
     ▼
┌──────────────────────────────────────────────────────┐
│  AutoTransactionHandler — BEFORE                     │
│                                                      │
│  1. Find [Transaction] attribute on action/class     │
│  2. Resolve ITransactionManager from DI              │
│     → TransactionManager (scoped to this request)   │
│  3. TransactionManager.BeginTransaction(isolation)   │
│     → ISession.BeginTransaction(isolation)           │
└──────────────────────────────────────────────────────┘
     │
     ▼
┌──────────────────────────────────────────────────────┐
│  Controller Action                                   │
│                                                      │
│  Constructor-injects ILinqRepository<Order, int>     │
│   → LinqRepository<Order, int>                      │
│       → NHibernateRepository (base)                 │
│           → INHibernateTransactionManager            │
│               → TransactionManager (scoped)         │
│                   → ISession (scoped)               │
│                       → ISessionFactory (singleton) │
│                                                      │
│  repo.GetAsync(id)   → Session.GetAsync(id)         │
│  repo.SaveAsync(e)   → Session.SaveAsync(e)         │
│  repo.FindAll(spec)  → Session.Query<T>()...        │
└──────────────────────────────────────────────────────┘
     │
     ▼
┌──────────────────────────────────────────────────────┐
│  AutoTransactionHandler — AFTER                      │
│                                                      │
│  Check ISupportsTransactionStatus.IsActive           │
│   ├── false → already closed, skip (log debug)      │
│   └── true  →                                       │
│       ├── exception || invalid model                 │
│       │     → RollbackTransactionAsync()            │
│       └── success                                   │
│             → CommitTransactionAsync(ct)            │
└──────────────────────────────────────────────────────┘
     │
     ▼
HTTP Response
```

---

## Testing Infrastructure

### Base Test Classes (xUnit)

#### `TransientDatabaseTests<TDatabaseInitializer>` (`SharpArch.Testing.Xunit.NHibernate`)

- Designed for **in-memory or transient databases** (e.g., SQLite in-memory).
- Creates the database schema before the test class runs (`TDatabaseInitializer`).
- Wraps **each test** in a transaction that is **rolled back** after the test completes.
- Ensures complete test isolation — no test leaves data behind.
- Inherits from xUnit `IAsyncLifetime` for setup/teardown.

#### `LiveDatabaseTests<TDatabaseSetup>` (`SharpArch.Testing.Xunit.NHibernate`)

- Designed for tests against a **real, pre-existing development database**.
- Does **not** run schema export — assumes schema is already up-to-date.
- Wraps each test in a transaction that is **rolled back** after the test.
- Useful for integration tests against a real SQL Server / PostgreSQL instance.
- Provided primarily for **backward compatibility**.

**Usage pattern:**

```csharp
public class OrderRepositoryTests : TransientDatabaseTests<SqliteTestDatabaseInitializer>
{
    readonly ISession _session;

    public OrderRepositoryTests(SqliteTestDatabaseInitializer dbInit)
        : base(dbInit) { }

    [Fact]
    public async Task Can_save_and_retrieve_order()
    {
        var repo = new NHibernateRepository<Order, int>(TransactionManager);
        var order = new Order { /* ... */ };
        await repo.SaveAsync(order);
        Session.Flush();

        var loaded = await repo.GetAsync(order.Id);
        Assert.NotNull(loaded);
    }
}
```

### Base Test Classes (NUnit)

Located in `SharpArch.Testing.NUnit/NHibernate/`:

| Class | Purpose |
|---|---|
| `RepositoryTestsBase` | NUnit equivalent of `TransientDatabaseTests`. Runs schema export before tests; each test rolls back. For in-memory / SQLite databases. |
| `DatabaseRepositoryTestsBase` | NUnit equivalent of `LiveDatabaseTests`. Tests against a live database; each test rolls back. |

Both inherit from NUnit's `[TestFixture]` pattern and use `[SetUp]` / `[TearDown]` for transaction management.

### General Test Helpers

Located in `SharpArch.Testing/Helpers/`:

- Framework-agnostic base types and utility helpers shared between NUnit and xUnit packages.
- Provides the core NHibernate session/transaction management logic that both `SharpArch.Testing.NUnit` and `SharpArch.Testing.Xunit.NHibernate` build upon.

#### `SetCultureAttribute` (`SharpArch.Testing.Xunit`)

An xUnit `BeforeAfterTestAttribute` that sets and restores `Thread.CurrentCulture` and `Thread.CurrentUICulture` for a specific test or test class:

```csharp
[SetCulture("fr-FR")]
public class PriceFormattingTests
{
    [Fact]
    public void Price_formats_with_french_locale() { /* ... */ }
}
```

---

## Sample Application — TardisBank

TardisBank is a pocket-money / savings application that demonstrates all Sharp-Architecture patterns in a complete end-to-end scenario.

### Project Structure

```
Samples/TardisBank/Src/
 ├── Suteki.TardisBank.Domain/          ← Domain layer
 │    ├── User.cs, Account.cs, ...      ← Entities extending Entity<int>
 │    ├── Money.cs, ...                 ← Value objects extending ValueObject
 │    └── [DomainSignature] properties  ← Business identity attributes
 │
 ├── Suteki.TardisBank.Infrastructure/  ← ORM / persistence layer
 │    ├── NHibernate mappings            ← FluentNHibernate ClassMap<T> or HBM
 │    └── Repository implementations    ← Concrete repos using NHibernateRepository
 │
 ├── Suteki.TardisBank.Tasks/           ← Application services layer
 │    └── Use-case orchestration        ← Commands / handlers that coordinate
 │         classes calling repositories  domain objects and repositories
 │
 ├── Suteki.TardisBank.Api/             ← Shared API contracts
 │    └── DTOs / request-response types  (not MVC controllers)
 │
 ├── Suteki.TardisBank.WebApi/          ← ASP.NET Core Web API
 │    ├── Controllers/                  ← [Transaction] attributed controllers
 │    └── Program.cs / Startup          ← AddNHibernateWithSingleDatabase wiring
 │
 └── Suteki.TardisBank.Tests/           ← Integration & unit tests
      └── Uses TransientDatabaseTests   ← In-memory DB, rollback per test
```

### Layer → Framework Dependency Mapping

| Sample Layer | Sharp-Architecture Package Used |
|---|---|
| Domain entities | `SharpArch.Domain` (`Entity<TId>`, `ValueObject`, `[DomainSignature]`) |
| Infrastructure / ORM | `SharpArch.NHibernate` (`NHibernateRepository`, `LinqRepository`, `EntityDuplicateChecker`) |
| Web API | `SharpArch.Web.AspNetCore` (`[Transaction]`, `AutoTransactionHandler`) |
| DI wiring | `SharpArch.NHibernate.DependencyInjection` (`AddNHibernateWithSingleDatabase`) |
| Tests | `SharpArch.Testing.Xunit.NHibernate` (`TransientDatabaseTests`) |

---

## Component Dependency Graph

```
SharpArch.Web.AspNetCore
 └─→ SharpArch.Domain
       (ITransactionManager, ISupportsTransactionStatus)

SharpArch.NHibernate.DependencyInjection
 ├─→ SharpArch.NHibernate
 │     (TransactionManager, NHibernateSessionFactoryBuilder)
 ├─→ SharpArch.Domain
 │     (ITransactionManager)
 └─→ SharpArch.Infrastructure
       (LogWrapper)

SharpArch.NHibernate
 ├─→ SharpArch.Domain
 │     (IEntity, IRepository, ILinqRepository,
 │      ITransactionManager, IEntityDuplicateChecker,
 │      ILinqSpecification, DomainSignatureAttribute, ValueObject, Enums)
 └─→ NHibernate + FluentNHibernate     (external NuGet)

SharpArch.Infrastructure
 └─→ Microsoft.Extensions.Logging      (external NuGet)

SharpArch.Domain
 └─→ (none — pure .NET BCL only)

SharpArch.Testing.Xunit.NHibernate
 ├─→ SharpArch.Testing
 ├─→ SharpArch.NHibernate
 └─→ xUnit                             (external NuGet)

SharpArch.Testing.NUnit
 ├─→ SharpArch.Testing
 ├─→ SharpArch.NHibernate
 └─→ NUnit                             (external NuGet)
```

---

## NuGet Package Dependencies

### `SharpArch.Domain.csproj`

```xml
<!-- No external NuGet dependencies -->
<!-- References only: System.ComponentModel.DataAnnotations (BCL) -->
```

### `SharpArch.NHibernate.csproj`

```xml
<PackageReference Include="NHibernate" />
<PackageReference Include="FluentNHibernate" />
<PackageReference Include="SharpArch.Domain" />
<PackageReference Include="SharpArch.Infrastructure" />
```

### `SharpArch.NHibernate.Extensions.DependencyInjection.csproj`

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
<PackageReference Include="SharpArch.NHibernate" />
<PackageReference Include="SharpArch.Infrastructure" />
```

### `SharpArch.Web.AspNetCore.csproj`

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Core" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
<PackageReference Include="SharpArch.Domain" />
```

---

*This document was generated from source-code analysis of the Sharp-Architecture repository (`Src/Lib` and `Src/Samples`). Keep this document in sync when adding new packages or significant API changes.*
