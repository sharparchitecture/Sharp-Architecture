// ADDINS
#addin nuget:?package=Cake.Coveralls&version=1.1.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.AppVeyor&version=5.0.1
#addin nuget:?package=Cake.ReSharperReports&version=0.11.1

// TOOLS
#tool nuget:?package=GitReleaseManager&version=0.11.0
#tool nuget:?package=GitVersion.CommandLine&version=5.6.7
#tool nuget:?package=coveralls.io&version=1.4.2
#tool nuget:?package=OpenCover&version=4.7.922
#tool nuget:?package=ReportGenerator&version=4.8.7
#tool nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2020.3.4

// ARGUMENTS
var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

var buildConfig = Argument("buildConfig", "Release");
if (string.IsNullOrEmpty(buildConfig)) {
    buildConfig = "Release";
}

// Build configuration

var repoOwner = "sharparchitecture";
var repoName = "Sharp-Architecture";

var local = BuildSystem.IsLocalBuild;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals($"{repoOwner}/{repoName}", AppVeyor.Environment.Repository.Name);

var isDebugBuild = string.Equals(buildConfig, "Debug", StringComparison.OrdinalIgnoreCase);
var isReleaseBuild = string.Equals(buildConfig, "Release", StringComparison.OrdinalIgnoreCase);

var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", AppVeyor.Environment.Repository.Branch);
var isReleaseBranch = AppVeyor.Environment.Repository.Branch.IndexOf("releases/", StringComparison.OrdinalIgnoreCase) >= 0
    || AppVeyor.Environment.Repository.Branch.IndexOf("hotfixes/", StringComparison.OrdinalIgnoreCase) >= 0;

var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;
var appVeyorJobId = AppVeyor.Environment.JobId;

// Solution settings

// Calculate version and commit hash
GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;
var nextMajorRelease = $"{semVersion.Major+1}.0.0";
var commitHash = semVersion.Sha;
var milestone = semVersion.MajorMinorPatch;

// Artifacts
var artifactsDir = "./Drops";
var artifactsDirAbsolutePath = MakeAbsolute(Directory(artifactsDir));

var testCoverageOutputFile = new FilePath(artifactsDir + "/OpenCover.xml");
var codeCoverageReportDir = artifactsDir + "/CodeCoverageReport";
var codeInspectionsOutputFile = artifactsDir + "/Inspections/CodeInspections.xml";
var duplicateFinderOutputFile = artifactsDir + "/Inspections/CodeDuplicates.xml";

var packagesDir = artifactsDir + "/packages";
var srcDir = "./Src";
var testsRootDir = srcDir + "/tests";
var solutionFile = new FilePath(srcDir + "/SharpArch.sln");
var samplesDir = "./Samples";
var coverageFilter="+[SharpArch*]* -[SharpArch.Tests*]* -[SharpArch.Xunit*]* -[SharpArch.Infrastructure]SharpArch.Infrastructure.Logging.*";

string githubToken = null;

// SETUP / TEARDOWN

Setup((context) =>
{
    Information("Building version {0} (tagged: {1}, local: {2}, release branch: {3})...", nugetVersion, isTagged, local, isReleaseBranch);
    CreateDirectory(artifactsDir);
    CleanDirectory(artifactsDir);
    githubToken = context.EnvironmentVariable("GITHUB_TOKEN");
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

Task("SetVersion")
    .Does(() =>
    {
        CreateAssemblyInfo($"{srcDir}/Common/AssemblyVersion.cs", new AssemblyInfoSettings{
            FileVersion = semVersion.MajorMinorPatch,
            InformationalVersion = semVersion.InformationalVersion,
            Version = semVersion.MajorMinorPatch
        });
    });


Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .ContinueOnError()
    .Does(() =>
    {
        AppVeyor.UpdateBuildVersion(buildVersion);
    });


Task("Restore")
    .DoesForEach(GetFiles(solutionFile.ToString()).Union(GetFiles($"{samplesDir}/**/*.sln")),
        (sln) => {
            Information("Running in {0}", sln.GetDirectory().FullPath);
            DotNetCoreRestore(sln.GetDirectory().FullPath);
        }
    );


Task("InspectCode")
    .Does(() => {
        DupFinder(solutionFile, new DupFinderSettings {
            CachesHome = "./tmp/DupFinderCaches",
            DiscardCost = 70,
            DiscardFieldsName = false,
            DiscardLiterals = false,
            NormalizeTypes = true,
            ShowStats = true,
            ShowText = true,
            OutputFile = duplicateFinderOutputFile,
            ExcludePattern = new string [] {
                "../Docker/**/*",
                "Solution Items/**/*",
                "Tests/**/*",
                "Samples/**/*"
            }
        });
        ReSharperReports(
            duplicateFinderOutputFile,
            System.IO.Path.ChangeExtension(duplicateFinderOutputFile, "html")
        );

        InspectCode(solutionFile, new InspectCodeSettings() {
            OutputFile = codeInspectionsOutputFile,
            Profile = "SharpArch.AutoLoad.DotSettings",
            CachesHome = "./tmp/ReSharperCaches",
            SolutionWideAnalysis = true
        });
        ReSharperReports(
            codeInspectionsOutputFile,
            System.IO.Path.ChangeExtension(codeInspectionsOutputFile, "html")
        );
    });


Task("RunXunitTests")
    .DoesForEach(GetFiles(solutionFile.ToString()).Union(GetFiles($"{samplesDir}/**/*.sln")),
    (testProj) => {
        var projectPath = testProj.GetDirectory();
        var projectFilename = testProj.GetFilenameWithoutExtension();
        Information("Calculating code coverage for {0} ...", projectFilename);

        var openCoverSettings = new OpenCoverSettings {
            OldStyle = true,
            ReturnTargetCodeOffset = 0,
            ArgumentCustomization = args => args.Append("-mergeoutput").Append("-hideskipped:File;Filter;Attribute"),
            WorkingDirectory = projectPath,
        }
        .WithFilter(coverageFilter)
        .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
        .ExcludeByFile("*/*Designer.cs");

        Func<string,ProcessArgumentBuilder> buildProcessArgs = (buildCfg) => {
				var pb = new ProcessArgumentBuilder()
                    .AppendSwitch("--configuration", buildCfg)
                    .AppendSwitch("--filter", "Category!=IntegrationTests")
                    .AppendSwitch("--results-directory", artifactsDirAbsolutePath.FullPath)
                    .Append("--no-build");
				if (!local) {
					pb.AppendSwitch("--test-adapter-path", ".")
						.AppendSwitch("--logger", $"AppVeyor");
				} else {
                     pb.AppendSwitch("--logger", $"trx;LogFileName={projectFilename}.trx");
                }
				return pb;
			};

        // run open cover for debug build configuration
        OpenCover(
            tool => tool.DotNetCoreTool(projectPath.FullPath,
                "test",
                buildProcessArgs("Debug")
            ),
            testCoverageOutputFile,
            openCoverSettings);

        // run tests again if Release mode was requested
        if (isReleaseBuild) {
            Information("Running Release mode tests for {0}", projectFilename.ToString());
            DotNetCoreTool(testProj.FullPath,
                "test",
                buildProcessArgs("Release")
            );
        }
    })
    .DeferOnError();


Task("CleanPreviousTestResults")
    .Does(() =>
    {
        if (FileExists(testCoverageOutputFile))
            DeleteFile(testCoverageOutputFile);
        DeleteFiles(artifactsDir + "/*.trx");
        if (DirectoryExists(codeCoverageReportDir))
            DeleteDirectory(codeCoverageReportDir, new DeleteDirectorySettings{
                Recursive = true,
                Force = true
            });
    });


Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, codeCoverageReportDir);
    });


Task("RunUnitTests")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    .IsDependentOn("RunXunitTests")
    .IsDependentOn("GenerateCoverageReport")
    .Does(() =>
    {
        Information("Done Test");
    })
    .Finally(() => {
        if (!local) {
            CoverallsIo(testCoverageOutputFile);
        }
    });


Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .DoesForEach(GetFiles($"{srcDir}/**/*.sln").Union(GetFiles($"{samplesDir}/**/*.sln")),
        (solutionFile) => {
            var slnPath = solutionFile.GetDirectory().FullPath;
            var sln = solutionFile.GetFilenameWithoutExtension();
            if (isReleaseBuild) {
                Information("Running {0} {1} build to calculate code coverage", sln, "Debug");
                // need Debug mode build for code coverage calculation
                DotNetCoreBuild(slnPath, new DotNetCoreBuildSettings {
                    NoRestore = true,
                    Configuration = "Debug",
                });
            }
            Information("Running {0} {1} build in {2}", sln, buildConfig, slnPath);
            DotNetCoreBuild(slnPath, new DotNetCoreBuildSettings {
                NoRestore = true,
                Configuration = buildConfig,
            });
        });


Task("CreateNugetPackages")
    .Does(() => {
        Action<string> buildPackage = (string projectName) => {
          var projectFileName=projectName;
          Information("Pack {0}", projectFileName);

          DotNetCorePack(projectFileName, new DotNetCorePackSettings {
              Configuration = buildConfig,
              OutputDirectory = packagesDir,
              NoBuild = true,
              NoRestore = true,
              ArgumentCustomization = args => args.Append($"-p:Version={nugetVersion}")
            });
        };

          if (isTagged) {
            var releaseNotes = $"https://github.com/{repoOwner}/{repoName}/releases/tag/{milestone}";
            Information("Updating ReleaseNotes Link: {0}", releaseNotes);
            XmlPoke("./Directory.Build.props",
              "/Project/PropertyGroup[@Label=\"Package\"]/PackageReleaseNotes",
              releaseNotes
            );
          }

        foreach(var projectName in new[] {$"{solutionFile}"}) {
            buildPackage(projectName);
        };
    });


Task("CreateRelease")
    .WithCriteria(() => isRepository && isReleaseBranch && !isPullRequest)
    .Does(() => {
        GitReleaseManagerCreate(githubToken, repoOwner, repoName,
            new GitReleaseManagerCreateSettings {
              Milestone = milestone,
              TargetCommitish = "master"
        });
    });


Task("CloseMilestone")
    .WithCriteria(() => isRepository && isTagged && !isPullRequest)
    .Does(() => {
        GitReleaseManagerClose(githubToken, repoOwner, repoName, milestone);
    });


Task("Default")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
//    .IsDependentOn("InspectCode")
    .IsDependentOn("CreateNugetPackages")
    .IsDependentOn("CreateRelease")
    .IsDependentOn("CloseMilestone")
    .Does(
        () => {}
    );


// EXECUTION
RunTarget(target);
