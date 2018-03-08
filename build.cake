// ADDINS
#addin "Cake.FileHelpers"
#addin "Cake.Coveralls"
#addin "Cake.PinNuGetDependency"
#addin "Cake.Incubator"
#addin "Cake.Issues"
#addin "Cake.Issues.InspectCode"
#addin "Cake.ReSharperReports"
#addin nuget:?package=Cake.AppVeyor
#addin nuget:?package=Refit&version=3.0.0
#addin nuget:?package=Newtonsoft.Json&version=9.0.1

// TOOLS
#tool "GitReleaseManager"
#tool "GitVersion.CommandLine"
#tool "coveralls.io"
#tool "OpenCover"
#tool "ReportGenerator"
#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=JetBrains.ReSharper.CommandLineTools"
#tool "nuget:?package=ReSharperReports"

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
var local = BuildSystem.IsLocalBuild;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals("SharpArchitecture/Sharp-Architecture", AppVeyor.Environment.Repository.Name);

var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", AppVeyor.Environment.Repository.Branch);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;
var appVeyorJobId = AppVeyor.Environment.JobId;


GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;

// SETUP / TEARDOWN

// Artifacts
var artifactDirectory = "./Drops/";
var testCoverageOutputFile = artifactDirectory + "OpenCover.xml";
var codeInspectionsOutputFile = artifactDirectory + "Inspections/CodeInspections.xml";
var duplicateFinderOutputFile = artifactDirectory + "Inspections/CodeDuplicates.xml";
var solutionFile = "./Solutions/SharpArch.sln";
var nunitTestResults = artifactDirectory + "Nunit3TestResults.xml";

Setup((context) =>
{
    Information("Building SharpArchitecture, version {0} (isTagged: {1}, isLocal: {2})...", nugetVersion, isTagged, local);
    CreateDirectory(artifactDirectory);
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

Task("SetVersion")
    .Does(() => 
    {
        CreateAssemblyInfo("./Common/AssemblyVersion.cs", new AssemblyInfoSettings{
            FileVersion = semVersion.MajorMinorPatch,
            InformationalVersion = semVersion.InformationalVersion,
            Version = semVersion.MajorMinorPatch
        });
    });
    

Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
    {
        AppVeyor.UpdateBuildVersion(buildVersion);

    }).ReportError(exception =>
    {  
        // When a build starts, the initial identifier is an auto-incremented value supplied by AppVeyor. 
        // As part of the build script, this version in AppVeyor is changed to be the version obtained from
        // GitVersion. This identifier is purely cosmetic and is used by the core team to correlate a build
        // with the pull-request. In some circumstances, such as restarting a failed/cancelled build the
        // identifier in AppVeyor will be already updated and default behaviour is to throw an
        // exception/cancel the build when in fact it is safe to swallow.
        // See https://github.com/reactiveui/ReactiveUI/issues/1262

        Warning("Build with version {0} already exists.", buildVersion);
    });


Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore("Solutions");
    });


Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("./Solutions/", new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = buildConfig,
        });
    });


Task("RunTests")
    .Does(() => 
    {
        var testAssemblies = GetFiles("./Solutions/tests/SharpArch.Tests/bin/Release/net462/SharpArch.Tests.dll")
            .Union(GetFiles("./Solutions/tests/SharpArch.Tests.NHibernate/bin/Release/net462/SharpArch.Tests.NHibernate.dll"))
            ;
        foreach (var item in testAssemblies)
        {
            Information("Test assembly: {0}", item);
        }

        Action<ICakeContext> testAction = tool => {
            tool.NUnit3(testAssemblies, 
                new NUnit3Settings {
                    OutputFile = artifactDirectory + "TestOutput.xml",
                    ErrorOutputFile = artifactDirectory + "ErrorOutput.xml",
                    Results = new [] {
                        new NUnit3Result {
                            FileName = nunitTestResults
                        }
                    },
                    ShadowCopy = false,
                });
        };

        OpenCover(testAction,
            testCoverageOutputFile,
            new OpenCoverSettings {
                ReturnTargetCodeOffset = 0,
                ArgumentCustomization = args => args.Append("-mergeoutput")
            }
            .WithFilter("+[*]* -[*.Tests*]*")
            .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
            .ExcludeByFile("*/*Designer.cs "));
    });


Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, artifactDirectory+"CodeCoverageReport");
    });


Task("UploadTestResults")
    .WithCriteria(() => !local)
    .Does(() => {
        CoverallsIo(testCoverageOutputFile);
        UploadFile("https://ci.appveyor.com/api/testresults/nunit3/"+appVeyorJobId, nunitTestResults);
    });


Task("RunUnitTests")
//    .IsDependentOn("Build")
    .IsDependentOn("RunTests")
    .IsDependentOn("GenerateCoverageReport")
    .IsDependentOn("UploadTestResults")
    .Does(() =>
    {
    });


Task("InspectCode")
    .Does(() => {
        DupFinder(solutionFile, new DupFinderSettings {
            DiscardCost = 70,
            DiscardFieldsName = false,
            DiscardLiterals = false,
            NormalizeTypes = true,
            ShowStats = true,
            ShowText = true,
            OutputFile = duplicateFinderOutputFile,
        });
        ReSharperReports(
		    duplicateFinderOutputFile, 
		    System.IO.Path.ChangeExtension(duplicateFinderOutputFile, "html")
        );

        InspectCode(solutionFile, new InspectCodeSettings() {
            OutputFile = codeInspectionsOutputFile,
            Profile = "./Solutions/SharpArch.sln.DotSettings",
            CachesHome = "./.ReSharperCaches",
            SolutionWideAnalysis = true
        });
        ReSharperReports(
		    codeInspectionsOutputFile, 
		    System.IO.Path.ChangeExtension(codeInspectionsOutputFile, "html")
        );
    });


Task("Default")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("InspectCode")
    .Does(
        () => {}
    );
    

// EXECUTION
RunTarget(target);
