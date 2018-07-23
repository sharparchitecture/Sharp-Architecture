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

// Solution settings
// Nuget packages to build
var nugetPackages = new [] {
    "SharpArch.Domain",
    "SharpArch.NHibernate",
    "SharpArch.RavenDb",
    "SharpArch.Testing",
    "SharpArch.Testing.NUnit",
    "SharpArch.Testing.Xunit",
    "SharpArch.Testing.Xunit.NHibernate",
    "SharpArch.Web.AspNetCore"
};

// Calculate version
GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;
var nextMajorRelease = $"{semVersion.Major+1}.0.0";
var commitHash = semVersion.Sha;

// Artifacts
var artifactsDir = "./Drops";
var testCoverageOutputFile = artifactsDir + "/OpenCover.xml";
var codeInspectionsOutputFile = artifactsDir + "/Inspections/CodeInspections.xml";
var duplicateFinderOutputFile = artifactsDir + "/Inspections/CodeDuplicates.xml";
var codeCoverageReportDir = artifactsDir + "/CodeCoverageReport";
var srcDir = "./Src";
var testsRootDir = srcDir + "/Tests";
var solutionFile = srcDir + "/SharpArch.sln";
var nunitTestResults = artifactsDir + "/Nunit3TestResults.xml";
var nugetTemplatesDir = "./NugetTemplates";
var nugetTempDir = artifactsDir + "/Packages";
var samplesDir = "./Samples";
var tardisBankSampleDir = samplesDir + "/TardisBank";

// SETUP / TEARDOWN

Setup((context) =>
{
    Information("Building SharpArchitecture, version {0} (isTagged: {1}, isLocal: {2})...", nugetVersion, isTagged, local);
    CreateDirectory(artifactsDir);
    CleanDirectory(artifactsDir);
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

Task("SetVersion")
    .Does(() =>
    {
        CreateAssemblyInfo("./Src/Common/AssemblyVersion.cs", new AssemblyInfoSettings{
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
        DotNetCoreRestore(srcDir);
        DotNetCoreRestore(tardisBankSampleDir+"/Src");
    });


Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = buildConfig,
        });
    });

Task("BuildSamples")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreBuild(tardisBankSampleDir + "/Src", new DotNetCoreBuildSettings{
            NoRestore = true,
            Configuration = buildConfig
        });
    });

Task("RunNunitTests")
    .Does(() =>
    {
        var testAssemblies = GetFiles($"{testsRootDir}/SharpArch.Tests/bin/{buildConfig}/net462/SharpArch.Tests.dll")
            .Union(GetFiles($"{testsRootDir}/SharpArch.Tests.NHibernate/bin/{buildConfig}/net462/SharpArch.Tests.NHibernate.dll"))
            ;
        foreach (var item in testAssemblies)
        {
            Information("NUnit test assembly: {0}", item);
        }

        Action<ICakeContext> testAction = tool => {
            tool.NUnit3(testAssemblies,
                new NUnit3Settings {
                    OutputFile = artifactsDir + "/TestOutput.xml",
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
            .WithFilter("+[SharpArch*]* -[SharpArch.Tests*]* -[SharpArch.xUnit*]*")
            .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
            .ExcludeByFile("*/*Designer.cs"));
    });



Task("RunXunitTests")
    .Does((ctx) =>
    {
        var testProjects = GetFiles($"{testsRootDir}/SharpArch.xUnit*/**/*.csproj");
        bool success = true;
        bool isDebugConfigurationRequested = string.Equals(buildConfig, "Debug", StringComparison.OrdinalIgnoreCase);

        foreach (var testProj in testProjects) {
            try
            {
                var projectPath = testProj.GetDirectory();
                var projectFilename = testProj.GetFilenameWithoutExtension();
                var openCoverSettings = new OpenCoverSettings {
                    OldStyle = true,
                    ReturnTargetCodeOffset = 0,
                    ArgumentCustomization = args => args.Append("-mergeoutput"),
                    WorkingDirectory = projectPath,
                }
                .WithFilter("+[SharpArch*]* -[SharpArch.Tests*]* -[SharpArch.xUnit*]*")
                .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
                .ExcludeByFile("*/*Designer.cs");

                var testOutputAbs = MakeAbsolute(File($"{artifactsDir}/xunitTests-{projectFilename}.xml"));

                var xunitArgs = isDebugConfigurationRequested
                    ? $"-xml {testOutputAbs} -c {buildConfig} --no-build")
                    : $"-xml {testOutputAbs} -c Debug");

                // run open cover for debug build configuration
                OpenCover(
                    tool => tool.DotNetCoreTool(projectPath.ToString(),
                        "xunit",
                        $"-xml {testOutputAbs} -c {buildConfig}"),
                    testCoverageOutputFile,
                    openCoverSettings);
                    
                // run tests again if Release mode was requested
                if (!isDebugConfigurationRequested) {
                    DotNetCoreTool(projectPath.ToString(),
                        "xunit",
                        $"-xml {testOutputAbs} -c {buildConfig}");
                }
            }
            catch (Exception ex)
            {
                Error("Error running tests", ex);
                success = false;
            }
        }
    });

Task("CleanPreviousTestResults")
    .Does(() =>
    {
        if (FileExists(testCoverageOutputFile))
            DeleteFile(testCoverageOutputFile);
        DeleteFiles(artifactsDir + "/xunitTests-*.xml");
        if (DirectoryExists(codeCoverageReportDir))
            DeleteDirectory(codeCoverageReportDir, recursive: true);
    });

Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, codeCoverageReportDir);
    });


Task("UploadTestResults")
    .WithCriteria(() => !local)
    .Does(() => {
        CoverallsIo(testCoverageOutputFile);
        //Information("Uploading nUnit result: {0}", nunitTestResults);
        //UploadFile("https://ci.appveyor.com/api/testresults/nunit3/"+appVeyorJobId, nunitTestResults);
        foreach(var xunitResult in GetFiles($"{artifactsDir}/xunitTests-*.xml"))
        {
            Information("Uploading xUnit results: {0}", xunitResult);
            UploadFile("https://ci.appveyor.com/api/testresults/xunit/"+appVeyorJobId, xunitResult);
        }
    });


Task("RunUnitTests")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    //.IsDependentOn("RunNunitTests")
    .IsDependentOn("RunXunitTests")
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
            Profile = "SharpArch.AutoLoad.DotSettings",
            CachesHome = "./.ReSharperCaches",
            SolutionWideAnalysis = true
        });
        ReSharperReports(
            codeInspectionsOutputFile,
            System.IO.Path.ChangeExtension(codeInspectionsOutputFile, "html")
        );
    });


Task("CreateNugetPackages")
    .Does(() => {
        // copy templates to temp folder
        CopyDirectory(nugetTemplatesDir, nugetTempDir);
        // update templates
        ReplaceTextInFiles(nugetTempDir+"/**/*.nuspec", "$(SemanticVersion)", nugetVersion);
        ReplaceTextInFiles(nugetTempDir+"/**/*.nuspec", "$(NextMajorRelease)", nextMajorRelease);
        ReplaceTextInFiles(nugetTempDir+"/**/*.nuspec", "$(CommitSHA)", commitHash);

        Func<string, string, string> removeBasePath = (path, basePath) => {
            var endOfBase = path.IndexOf(basePath);
            if (endOfBase < 0)
                return path; // base not found
            endOfBase += basePath.Length;
            return path.Substring(endOfBase);
        };

        Action<string> buildPackage = (string projectName) => {
            Information("Creating package {0}", projectName);
            var files = GetFiles($"{srcDir}/{projectName}/bin/Release/**/{projectName}.*");
            var filePathes = files.Where(f=> !f.FullPath.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
                .Select(filePath => removeBasePath(filePath.FullPath, $"{projectName}/bin/Release/"));

            // create folders
            foreach(var frameworkLib in filePathes.Select(fp => new FilePath(fp).GetDirectory().FullPath).Distinct()) {
                CreateDirectory($"{nugetTempDir}/{projectName}/lib/{frameworkLib}");
            };

            foreach (var file in filePathes) {
                var srcFile = $"{srcDir}/{projectName}/bin/Release/{file}";
                var dstFile = $"{nugetTempDir}/{projectName}/lib/{file}";
                CopyFile(srcFile, dstFile);
            };

            var exitCode = StartProcess("nuget.exe", new ProcessSettings {
                WorkingDirectory = $"{nugetTempDir}/{projectName}",
                Arguments = "pack -OutputDirectory .."
            });
            if (exitCode != 0)
                throw new Exception($"Build package {projectName} failed with code {1}.");
        };

        foreach(var projectName in nugetPackages) {
            buildPackage(projectName);
        };
    });

Task("PublishNugetPackages")
    .IsDependentOn("CreateNugetPackages")
    .Does(() => {

    });

Task("Default")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("InspectCode")
    .IsDependentOn("CreateNugetPackages")
    .IsDependentOn("BuildSamples")
    .Does(
        () => {}
    );


// EXECUTION
RunTarget(target);
