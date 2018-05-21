# S#arp Architecture


| | Stable | Develop |
|:--:|:--:|:--:|
| Build | [![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p/branch/master?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture) |
| NuGet | [![NuGet](https://img.shields.io/nuget/v/Sharp-Architecture.svg)](https://www.nuget.org/packages?q=SharpArch)|[![NuGet](https://img.shields.io/nuget/vpre/Sharp-Architecture.svg)](https://www.nuget.org/packages?q=SharpArch) |
| Coverage | [![Coverage Status](https://coveralls.io/repos/github/sharparchitecture/Sharp-Architecture/badge.svg?branch=master)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture?branch=master) | [![Coverage Status](https://coveralls.io/repos/github/sharparchitecture/Sharp-Architecture/badge.svg?branch=develop)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture?branch=develop) |


## Samples


To learn how to get a S#arp Architecture solution up and running, please check the samples https://github.com/sharparchitecture/Sharp-Architecture/tree/develop/Samples.
**Note: Samples for version 5 are not ready yet.**


## Downloads


Downloads can always be found here: https://github.com/sharparchitecture/Sharp-Architecture/downloads


## Building S#arp Architecture


Perform the following command in GitBash:
```Shell
$ git clone git@github.com:sharparchitecture/Sharp-Architecture.git
$ git checkout develop
$ powershell -file build.ps1 -Target=RunUnitTests
```
Now you should have the latest development branch of S#arp Architecture.


## Documentation and Assemblies


* /Artefacts/Documentation/:  Contains a link to comprehensive, online documentation at http://sharp-architecture.readthedocs.org/, and a diagram of what a S#arp Architecture project looks like.


## How's this release organized?


* /Artefacts/:  Contains various artefacts for the project.

* /Common/: Contains files shared among the projects.

* /NugetTemplates/: Contains templates used for generating S#arp Architecture nuget packages.

* /Packages/: Contains the NuGet packages that S#arp Architecture depends on.

* /License.txt:  I'll let you guess what this is.

* /Src/: This contains all of the source code in their various projects.

* /VersionHistory.txt:  Details version numbers of dependencies, changes since previous releases, upgrade details, and a roadmap of what's coming.
