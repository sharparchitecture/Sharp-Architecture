# S#arp Architecture


| | Stable | Develop |
|:--:|:--:|:--:|
| Build | [![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p/branch/master?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture) |
| NuGet | [![NuGet](https://img.shields.io/nuget/v/SharpArch.Domain.svg)](https://www.nuget.org/packages?q=SharpArch)|[![NuGet](https://img.shields.io/nuget/vpre/SharpArch.Domain.svg)](https://www.nuget.org/packages?q=SharpArch) |
| Coverage | [![Coverage Status](https://coveralls.io/repos/github/sharparchitecture/Sharp-Architecture/badge.svg?branch=master)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture?branch=master) | [![Coverage Status](https://coveralls.io/repos/github/sharparchitecture/Sharp-Architecture/badge.svg?branch=develop)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture?branch=develop) |


## Samples


To learn how to get a S#arp Architecture solution up and running, please check the samples https://github.com/sharparchitecture/Sharp-Architecture/tree/develop/Samples.
**Note: Samples for version 5 are not ready yet.**


## Downloads


Downloads can always be found here: https://github.com/sharparchitecture/Sharp-Architecture/downloads


## Building S#arp Architecture


The build is driven by [Cake](https://cakebuild.net) and requires only the .NET SDK (see [global.json](global.json) for the pinned version, currently 11.0.100). AppVeyor CI installs that SDK on both Linux and Windows build images with the official `dotnet-install` script, so bumping `global.json` is all it takes to move CI to a new SDK.

```Shell
$ git clone git@github.com:sharparchitecture/Sharp-Architecture.git
$ git checkout develop
$ dotnet tool install Cake.Tool --global   # once
$ dotnet cake                               # full pipeline: build + tests + coverage + NuGet packages
```

To build and run the unit tests only:

```Shell
$ dotnet cake --target=RunUnitTests
```

Unit tests run against an in-memory SQLite database and need no setup. Tests tagged `IntegrationTests`, `Functional` or `ManualTests` require a real database; start one with `pwsh Docker/start-mssql.ps1` (SQL Server on port 2433) or `pwsh Docker/start-postgres.ps1` — see [Running SQL Server in Docker](docs/getting-started/mssql-in-docker.md).

The solution file is [Src/SharpArch.slnx](Src/SharpArch.slnx) (the new XML-based solution format, supported by Visual Studio 2026+, recent JetBrains Rider versions and current .NET SDKs).


## Documentation and Assemblies


Documentation lives as Markdown in the [/docs/](docs/README.md) folder and is published via GitHub Pages at <https://sharparchitecture.github.io/Sharp-Architecture/>. It is organized into:

* [General](docs/README.md) — overview, architecture, unit of work, domain-signature source generator, FAQ.
* [Getting started](docs/getting-started/installation.md) — installation and building a simple CRUD application.
* [NHibernate](docs/nhibernate/nhibernate.md) — NHibernate integration, HiLo id generator, multiple databases.
* [Reference](docs/changelog.md) — changelog, release workflow, additional resources.

The framework itself ships as a set of `SharpArch.*` NuGet packages built from [/Src/Lib/](Src/Lib):

* `SharpArch.Domain` — the core domain building blocks and persistence-support interfaces (no infrastructure dependencies).
* `SharpArch.NHibernate` / `SharpArch.NHibernate.DependencyInjection` — the NHibernate implementation and DI wiring.
* `SharpArch.Web.AspNetCore` — ASP.NET Core MVC integration (per-request transactions and model validation).
* `SharpArch.Infrastructure` — cross-cutting helpers.
* `SharpArch.Testing[.Xunit|.NUnit][.NHibernate]` — base classes for consumers' tests.
* `SharpArch.SourceGenerators.DomainSignature` — analyzer/source generator for `[DomainSignature]`.


## How's this release organized?


* /Src/ — All source code.
    * /Src/Lib/ — The shipped `SharpArch.*` NuGet packages.
    * /Src/Tests/ — Unit and integration tests for the packages.
    * /Src/Samples/ — Sample applications; TardisBank is the end-to-end reference.
    * /Src/Common/ — Files shared across the projects (e.g. AssemblyVersion.cs).

* /docs/ — Markdown documentation, published via GitHub Pages.

* /.build/, build.cake, build.sh — Cake build scripts and the Unix bootstrapper.

* /Docker/ — Scripts to spin up SQL Server / PostgreSQL containers for the integration tests.

* /Artefacts/ — Logo/branding assets and code-style configurations.

* /Old/ — Legacy pre-v5 samples kept for reference only; do not use them as a pattern for new work.

* /VersionHistory.txt — Details changes since the previous releases, and upgrade notes.

* appveyor.yml, GitVersion.yml, GitReleaseManager.yaml — CI and release configuration (AppVeyor, GitVersion, GitReleaseManager). Versions are derived from git history, never hardcoded.

* /LICENSE — License terms.
