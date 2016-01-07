[![Master branch build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p/branch/master?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture/branch/master)

[![Develop branch build status](https://ci.appveyor.com/api/projects/status/q90e3hg7g3wgf79p/branch/develop?svg=true)](https://ci.appveyor.com/project/sharparchitecture/sharp-architecture/branch/develop)

--------------------------------------------
Templify
--------------------------------------------

We no longer support the use of the Visual Studio templates for installation of S#arp Architecture. With version 2.0, we have dropped support for Visual Studio 2008 altogether. To learn how to get a S#arp Architecture solution up and running, please go to http://blog.sharparchitecture.net/post/Using-Templify-to-create-a-new-Sarp-Architecture-solution.aspx

--------------------------------------------
Downloads
--------------------------------------------

Downloads can always be found here: https://github.com/sharparchitecture/Sharp-Architecture/downloads

--------------------------------------------
Building S#arp Architecture
--------------------------------------------

Perform the following command in GitBash:

$ git clone git@github.com:sharparchitecture/Sharp-Architecture.git
$ git submodule init
$ git submodule update

Now you should have the latest development branch of SA 2.0 and submodules.

Now go to /Build and run the Build.cmd or BuildAndPackage.cmd files to build S#arp Architecture

--------------------------------------------
Documentation and Assemblies
--------------------------------------------

* /Artefacts/Documentation/:  Contains a link to comprehensive, online documentation at http://wiki.sharparchitecture.net/, and a diagram of what a S#arp Architecture project looks like.

* /Drops/<Version Number>/:  Holds released SharpArch assemblies - it does not include the third party library's that are needed by S#arp Architecture. You must run the BuildAndPackage.cmd file first.

--------------------------------------------
How's this release organized?
--------------------------------------------

* /Artefacts/:  Contains various artefacts for the project.

* /Build/:  Contains the .cmd files needed to build the solution.

* /Common/: Contains files shared among the projects.

* /NugetTemplates/: Contains templates used for generating S#arp Architecture nuget packages.

* /ReferencedAssemblies/:  Contains all of the required 3rd party assemblies. 

* /Packages/: Contains the NuGet packages that S#arp Architecture depends on.

* /License.txt:  I'll let you guess what this is.

* /Solutions/: This contains all of the source code in their various projects.

* /VersionHistory.txt:  Details version numbers of dependencies, changes since previous releases, upgrade details, and a roadmap of what's coming.
