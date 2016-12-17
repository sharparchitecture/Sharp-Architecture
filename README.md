# S#arp Architecture

||Stable|Pre-release|
|:--:|:--:|:--:|
|Build|[![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p/branch/master?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture/branch/master)|[![Build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture)|
|NuGet|[![NuGet](https://img.shields.io/nuget/v/Sharp-Architecture.svg)](https://www.nuget.org/packages?q=SharpArch)|[![NuGet](https://img.shields.io/nuget/vpre/Sharp-Architecture.svg)](https://www.nuget.org/packages?q=SharpArch)|
|Coverage|[![Coveralls](https://img.shields.io/coveralls/sharparchitecture/Sharp-Architecture.svg)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture)|[![Coveralls](https://img.shields.io/coveralls/sharparchitecture/Sharp-Architecture/develop.svg)](https://coveralls.io/github/sharparchitecture/Sharp-Architecture)|


## Templify / Visual Studio templates

Starting with version 4 solution templates are not maintained anymore. Main reason for this is a variaty of projects can be build based on S#Aarp Architecture. 
As usual, pull requests are welcome.

To learn how to get a S#arp Architecture solution up and running, please check the samples https://github.com/sharparchitecture/Sharp-Architecture/tree/develop/Samples.

## Downloads

Downloads can always be found here: https://github.com/sharparchitecture/Sharp-Architecture/downloads

## Building S#arp Architecture

Perform the following command in GitBash:
```Shell
$ git clone git@github.com:sharparchitecture/Sharp-Architecture.git
$ git submodule init
$ git submodule update
```
Now you should have the latest development branch of S#arp Architecture and submodules.

Now go to /Build and run the Build.cmd or BuildAndPackage.cmd files to build S#arp Architecture. The desktop build uses version *0.1.0* by default. Build server uses GitVersion[https://github.com/GitTools/GitVersion] 
to generate Semantic Version.


## Documentation and Assemblies


* /Artefacts/Documentation/:  Contains a link to comprehensive, online documentation at http://sharp-architecture.readthedocs.org/, and a diagram of what a S#arp Architecture project looks like.

* /Drops/<Version Number>/:  Holds released SharpArch assemblies - it does not include the third party library's that are needed by S#arp Architecture. You must run the BuildAndPackage.cmd file first.


## How's this release organized?

* /Artefacts/:  Contains various artefacts for the project.

* /Build/:  Contains the .cmd files needed to build the solution.

* /Common/: Contains files shared among the projects.

* /NugetTemplates/: Contains templates used for generating S#arp Architecture nuget packages.

* /Packages/: Contains the NuGet packages that S#arp Architecture depends on.

* /License.txt:  I'll let you guess what this is.

* /Solutions/: This contains all of the source code in their various projects.

* /VersionHistory.txt:  Details version numbers of dependencies, changes since previous releases, upgrade details, and a roadmap of what's coming.
