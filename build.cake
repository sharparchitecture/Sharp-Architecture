// ADDINS
#addin "Cake.FileHelpers"
#addin "Cake.Coveralls"
#addin "Cake.Issues"
#addin "Cake.Issues.InspectCode"
#addin "Cake.ReSharperReports"
#addin nuget:?package=Cake.AppVeyor
#addin nuget:?package=Refit
#addin nuget:?package=Newtonsoft.Json

// TOOLS
#tool "GitReleaseManager"
#tool "GitVersion.CommandLine"
#tool "coveralls.io"
#tool "OpenCover"
#tool "ReportGenerator"
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

var isDebugBuild = string.Equals(buildConfig, "Debug", StringComparison.OrdinalIgnoreCase);
var isReleaseBuild = string.Equals(buildConfig, "Release", StringComparison.OrdinalIgnoreCase);

var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", AppVeyor.Environment.Repository.Branch);
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;
var appVeyorJobId = AppVeyor.Environment.JobId;

// Solution settings
// Nuget packages to build
var nugetPackages = new [] {
    "SharpArch.Domain",
    "SharpArch.Infrastructure",
    "SharpArch.NHibernate",
    "SharpArch.RavenDb",
    "SharpArch.Testing",
    "SharpArch.Testing.NUnit",
    "SharpArch.Testing.Xunit",
    "SharpArch.Testing.Xunit.NHibernate",
    "SharpArch.Web.AspNetCore"
};

// Calculate version and commit hash
GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;
var nextMajorRelease = $"{semVersion.Major+1}.0.0";
var commitHash = semVersion.Sha;

// Artifacts
var artifactsDir = "./Drops";
var artifactsDirAbsolutePath = MakeAbsolute(Directory(artifactsDir));
var testCoverageOutputFile = artifactsDir + "/OpenCover.xml";
var codeInspectionsOutputFile = artifactsDir + "/Inspections/CodeInspections.xml";
var duplicateFinderOutputFile = artifactsDir + "/Inspections/CodeDuplicates.xml";
var codeCoverageReportDir = artifactsDir + "/CodeCoverageReport";
var srcDir = "./Src";
var testsRootDir = srcDir + "/Tests";
var solutionFile = srcDir + "/SharpArch.sln";
var nugetTemplatesDir = "./NugetTemplates";
var nugetTempDir = artifactsDir + "/Packages";
var samplesDir = "./Samples";
var tardisBankSampleDir = samplesDir + "/TardisBank";
var nugetExe = "./tools/nuget.exe";

// SETUP / TEARDOWN

Setup((context) =>
{
    Information("Building SharpArchitecture, version {0} (isTagged: {1}, isLocal: {2})...", nugetVersion, isTagged, local);
    CreateDirectory(artifactsDir);
    CleanDirectory(artifactsDir);
    if (!FileExists(nugetExe)) {
        Information("Downloading Nuget.exe ...");
        DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", nugetExe);
    }
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
        DotNetCoreRestore(tardisBankSampleDir + "/Src");
    });


Task("Build")
    .IsDependentOn("BuildLibrary")
    .IsDependentOn("BuildSamples")
    .Does(() => { });

Task("BuildLibrary")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        if (isReleaseBuild) {
            Information("Running {0} build for code coverage", "Debug");
            // need Debug build for code coverage
            DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
                NoRestore = true,
                Configuration = "Debug",
            });
        }
        Information("Running {0} build for code coverage", buildConfig);
        DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = buildConfig,
        });
    });

Task("BuildSamples")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        if (isReleaseBuild) {
            Information("Running {0} build for code coverage", "Debug");
            DotNetCoreBuild(tardisBankSampleDir + "/Src", new DotNetCoreBuildSettings{
                Configuration = "Debug"
            });
        }
        DotNetCoreBuild(tardisBankSampleDir + "/Src", new DotNetCoreBuildSettings{
            Configuration = buildConfig
        });
    });

Task("RunUnitTests")
    .DoesForEach(GetFiles($"{testsRootDir}/**/*.csproj")
        .Union(GetFiles($"{samplesDir}/**/**/*.Tests.csproj")), 
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
        .WithFilter("+[SharpArch*]* -[SharpArch.Tests*]* -[SharpArch.Xunit*]* -[SharpArch.Infrastructure]SharpArch.Infrastructure.Logging.*")
        .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
        .ExcludeByFile("*/*Designer.cs");

        Func<string,ProcessArgumentBuilder> buildProcessArgs = (buildCfg) =>
            new ProcessArgumentBuilder()
                    .AppendSwitch("--configuration", buildCfg)
                    .AppendSwitch("--filter", "Category!=IntegrationTests")
                    .AppendSwitch("--results-directory", artifactsDirAbsolutePath.FullPath)
                    .AppendSwitch("--logger", $"trx;LogFileName={projectFilename}.trx")
                    .Append("--no-build");

        // run open cover for debug build configuration
        OpenCover(
            tool => tool.DotNetCoreTool(projectPath.ToString(),
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
            DeleteDirectory(codeCoverageReportDir, recursive: true);
    });

Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, codeCoverageReportDir);
    });


Task("Test")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("GenerateCoverageReport")
    .Does(() =>
    {
        Information("Done Test");
    })
    .Finally(() => {
        if (!local) {
            CoverallsIo(testCoverageOutputFile);

            foreach(var trxFile in GetFiles($"{artifactsDir}/*.trx"))
            {
                Information("Uploading unit-test results: {0}", trxFile);
                UploadFile("https://ci.appveyor.com/api/testresults/mstest/" + appVeyorJobId, trxFile);
            }
        }
    });


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

            var exitCode = StartProcess(nugetExe, new ProcessSettings {
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
    .IsDependentOn("BuildSamples")
    .IsDependentOn("Test")
    .IsDependentOn("InspectCode")
    .IsDependentOn("CreateNugetPackages")
    .Does(
        () => {}
    );


// EXECUTION
RunTarget(target);
