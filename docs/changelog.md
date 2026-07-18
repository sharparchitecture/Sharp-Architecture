# Changelog

The full release history — every breaking change, enhancement, dependency bump, and bug
fix since S#arp Architecture 0.8 — is kept in
[`VersionHistory.txt`](../VersionHistory.txt) at the repository root. It is updated as
part of every release, so it's the source of truth; this page is just a map to it.

## Current release

**9.0.0** is the latest tagged release (matches `master`, tag
[`9.0.0`](https://github.com/sharparchitecture/Sharp-Architecture/releases/tag/9.0.0)).
`Src/Directory.Build.props` already points `PackageReleaseNotes` at a `9.1.0` release
tag, so an untagged `9.1.0` is in progress on top of it — nothing in this documentation
depends on the difference, since no breaking changes have landed since `9.0.0` shipped.

- **Breaking:** dropped .NET 5, .NET Core 3.1, and .NET Standard 2.0 support
- **Breaking:** discontinued the `SharpArch.RavenDB` library — NHibernate is now the
  only supported persistence provider (see [NHibernate](nhibernate/nhibernate.md))
- Added .NET 8 and .NET 9 support
- Updated code to use C# 10 features
- Replaced FluentAssertions with Shouldly in the test suites

## Recent major versions

| Version | Highlights |
|---|---|
| 8.0.1 | .NET 6 support finalized (RC → release) |
| 8.0.0 | Dropped .NET Core 2.1; moved shared build config to `Directory.Build.props`; Linux + Windows CI |
| 7.0.0 | **Breaking:** removed `EntityWithTypedId<>` in favor of `Entity<TId>`; renamed `IAsyncRepository` to `IRepository` |
| 6.1.x | .NET 5 support |

For anything older — including the ASP.NET MVC / RavenDB / Castle Windsor era covered
by the pre-9.0 docs — see the full [`VersionHistory.txt`](../VersionHistory.txt).

## Target frameworks

Defined centrally in [`Src/Directory.Build.props`](../Src/Directory.Build.props):

- **Libraries** (`Src/Lib/`): `netstandard2.1`, `net8.0`, `net9.0`
- **Apps & test projects**: `net8.0`, `net9.0`
