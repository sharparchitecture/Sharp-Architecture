# Changelog

The full release history — every breaking change, enhancement, dependency bump, and bug
fix since S#arp Architecture 0.8 — is kept in
[`VersionHistory.txt`](../VersionHistory.txt) at the repository root. It is updated as
part of every release, so it's the source of truth; this page is just a map to it.

## Current release

**9.1.0** is the latest tagged release (tag
[`9.1.0`](https://github.com/sharparchitecture/Sharp-Architecture/releases/tag/9.1.0)).

- Added .NET 10 support
- Upgraded xUnit to v3 for .NET 8+ targets
- Upgraded NHibernate to 5.6.1 and refreshed other dependencies

## In progress (vNext)

Unreleased work on top of `9.1.0`, tracked in the `vNext` section of
[`VersionHistory.txt`](../VersionHistory.txt). No breaking changes so far.

- Added .NET 11 support (`net11.0` target); the .NET SDK is pinned to `11.0.100` in
  [`global.json`](../global.json)
- AppVeyor CI (Linux and Windows) installs the SDK pinned in `global.json`, because the
  build images ship only up to .NET 10
- Upgraded NHibernate to 5.7.1 and refreshed other dependencies

## Recent major versions

| Version | Highlights |
|---|---|
| 9.1.0 | .NET 10 support; xUnit v3 for .NET 8+ targets |
| 9.0.0 | **Breaking:** dropped .NET 5, .NET Core 3.1 and .NET Standard 2.0; discontinued `SharpArch.RavenDB`; added .NET 8 and .NET 9; Shouldly replaces FluentAssertions |
| 8.0.1 | .NET 6 support finalized (RC → release) |
| 8.0.0 | Dropped .NET Core 2.1; moved shared build config to `Directory.Build.props`; Linux + Windows CI |
| 7.0.0 | **Breaking:** removed `EntityWithTypedId<>` in favor of `Entity<TId>`; renamed `IAsyncRepository` to `IRepository` |
| 6.1.x | .NET 5 support |

For anything older — including the ASP.NET MVC / RavenDB / Castle Windsor era covered
by the pre-9.0 docs — see the full [`VersionHistory.txt`](../VersionHistory.txt).

## Target frameworks

Defined centrally in [`Src/Directory.Build.props`](../Src/Directory.Build.props):

- **Libraries** (`Src/Lib/`): `netstandard2.1`, `net8.0`, `net9.0`, `net10.0`, `net11.0`
- **Apps & test projects**: `net8.0`, `net9.0`, `net10.0`, `net11.0`

Building requires the .NET SDK pinned in [`global.json`](../global.json) (currently
`11.0.100`).
