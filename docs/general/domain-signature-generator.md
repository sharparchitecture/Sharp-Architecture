# Domain signature source generator

The `SharpArch.SourceGenerators.DomainSignature` package ships a Roslyn source
generator that implements `GetHashCode()` and `HasSameObjectSignatureAs(...)`
for your entities at compile time, based on the properties you mark with
`[DomainSignature]`.

Compared to the default runtime reflection (see
[below](#relationship-to-the-runtime-signature)), the generated code:

- has no reflection cost on the equality and hashing hot paths,
- needs no per-type property metadata caching,
- fails fast at build time with actionable diagnostics.

## Install

```bash
dotnet add package SharpArch.SourceGenerators.DomainSignature
```

Both attributes (`[GenerateDomainSignature]` and `[DomainSignature]`) ship with
[`SharpArch.Domain`](https://www.nuget.org/packages/SharpArch.Domain/) — no
extra package is needed for them.

## Enabling generation

Decorate a **partial** entity class with `[GenerateDomainSignature]` and mark
its business-identity properties with `[DomainSignature]`:

```csharp
using SharpArch.Domain.DomainModel;

[GenerateDomainSignature]
public partial class Customer : Entity<int>
{
    [DomainSignature]
    public string Name { get; set; } = null!;

    [DomainSignature]
    public string Email { get; set; } = null!;

    // Not part of the signature.
    public string? Notes { get; set; }
}
```

The generator emits a companion partial part overriding `GetHashCode()` and
`HasSameObjectSignatureAs(...)`, comparing exactly the `[DomainSignature]`
properties — reference types, nullable value types and value types are
compared differently, each with the correct null handling. Records are
supported and emit a `partial record`.

## Requirements

| Requirement | Diagnostic if violated |
|---|---|
| Class must be declared `partial` | `SHARPARCH001` |
| Class must derive from `BaseObject` (e.g. via `Entity<TId>`) | `SHARPARCH002` |
| At least one `[DomainSignature]` property | `SHARPARCH003` (warning) |
| Class must be declared in a (non-global) namespace | `SHARPARCH004` |
| Class must not be nested | `SHARPARCH005` |
| Signature property types must be reference, value or nullable value types | `SHARPARCH006` |

## Diagnostics

| ID | Severity | Meaning |
|---|---|---|
| `SHARPARCH001` | Error | Class must be declared as partial |
| `SHARPARCH002` | Error | Class must derive from BaseObject |
| `SHARPARCH003` | Warning | No `[DomainSignature]` properties found |
| `SHARPARCH004` | Error | Class must be declared in a namespace |
| `SHARPARCH005` | Error | Class must not be nested |
| `SHARPARCH006` | Error | A signature property has an unsupported type (pointer, ref struct or generic type parameter) |

## Relationship to the runtime signature

Without `[GenerateDomainSignature]`, `Entity<TId>` discovers `[DomainSignature]`
properties via reflection (cached per type). The two paths are kept in sync:
pointer and ref struct signature properties are rejected on both — by
`SHARPARCH006` at build time and by an `InvalidOperationException` at runtime —
while generic type parameters can only occur at compile time and are therefore
rejected by the generator alone.

> **Note:** the duplicate checker (`IEntityDuplicateChecker`) always uses the
> reflection-based signature, so keep `[DomainSignature]` accurate regardless
> of whether you enable generation.
