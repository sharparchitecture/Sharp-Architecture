// ADDINS
#addin "Cake.FileHelpers"
#addin "Cake.Coveralls"
#addin "Cake.PinNuGetDependency"
#addin "Cake.Incubator"

// TOOLS
#tool "GitReleaseManager"
#tool "GitVersion.CommandLine"
#tool "coveralls.io"
#tool "OpenCover"
#tool "ReportGenerator"
#tool "nuget:?package=NUnit.ConsoleRunner"

// ARGUMENTS
var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

var buildConfig = Argument("buildConfig", "Debug");
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

GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;

// SETUP / TEARDOWN

// Artifacts
var artifactDirectory = "./Drops/";
var testCoverageOutputFile = artifactDirectory + "OpenCover.xml";


Setup((context) =>
{
    Information("Building SharpArchitecture, version {0} (isTagged: {1})...", nugetVersion, isTagged);
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
    
    
Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore("Solutions");
    });


Task("Build")
    .Does(() =>
    {
        DotNetCoreBuild("./Solutions/", new DotNetCoreBuildSettings{

        });
    });


Task("RunUnitTests")
//    .IsDependentOn("Build")
    .Does(() =>
    {
        var testAssemblies = GetFiles("./Solutions/tests/SharpArch.Tests/bin/Release/net462/SharpArch.Tests.dll")
            //.Union(GetFiles("./Solutions/tests/SharpArch.Tests.NHibernate/bin/Release/net462/SharpArch.Tests.NHibernate.dll"))
            ;

        foreach (var item in testAssemblies)
        {
            Information("Test assembly: {0}", item);
        }

        Information("Artifacts directory: {0}", artifactDirectory + "TestResult.xml");
        NUnit3(testAssemblies, 
            new NUnit3Settings {
                OutputFile = artifactDirectory + "TestOutput.xml",
                ErrorOutputFile = artifactDirectory + "ErrorOutput.xml",
                Results = new [] {
                    new NUnit3Result {
                        FileName = artifactDirectory + "TestResult.xml"
                    }
                },
                //OutputDirectory = artifactDirectory + "/",
                ShadowCopy = false,
            });


        // Action<ICakeContext> testAction = tool => {
        //     tool.NUnit3("./Solutions/tests/**/bin/Release/**/SharpArch.Tests.*.dll", 
        //         new NUnit3Settings {
        //             //OutputDirectory = artifactDirectory + "/",
        //             ShadowCopy = false,
        //         });
        // };

        // OpenCover(testAction,
        //     testCoverageOutputFile,
        //     new OpenCoverSettings {
        //         ReturnTargetCodeOffset = 0,
        //         ArgumentCustomization = args => args.Append("-mergeoutput")
        //     }
        //     .WithFilter("+[*]* -[*.Tests*]* -[Splat*]*")
        //     .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
        //     .ExcludeByFile("*/*Designer.cs;*/*.g.cs;*/*.g.i.cs;*splat/splat*"));

        // ReportGenerator(testCoverageOutputFile, artifactDirectory);
    });


Task("Default")
    .IsDependentOn("SetVersion")
    // .IsDependentOn("Restore")
    // .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .Does(
        () => {}
    );
    
// EXECUTION
RunTarget(target);
