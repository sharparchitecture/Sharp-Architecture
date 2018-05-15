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
    "SharpArch.Web.AspNetCore",
    "SharpArch.Testing.NUnit" 
};

GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;
var nextMajorRelease = $"{semVersion.Major+1}.0.0";

// SETUP / TEARDOWN

// Artifacts
var artifactDirectory = "./Drops";
var testCoverageOutputFile = artifactDirectory + "/OpenCover.xml";
var codeInspectionsOutputFile = artifactDirectory + "/Inspections/CodeInspections.xml";
var duplicateFinderOutputFile = artifactDirectory + "/Inspections/CodeDuplicates.xml";
var solutionsFolder = "./Solutions";
var solutionFile = solutionsFolder + "/SharpArch.sln";
var nunitTestResults = artifactDirectory + "/Nunit3TestResults.xml";
var nugetTemplates = "./NugetTemplates";
var nugetTemp = artifactDirectory + "/Packages";

Setup((context) =>
{
    Information("Building SharpArchitecture, version {0} (isTagged: {1}, isLocal: {2})...", nugetVersion, isTagged, local);
    CreateDirectory(artifactDirectory);
    CleanDirectory(artifactDirectory);
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
    .Does((ctx) => 
    {
		
		var testProjects = GetFiles("./Solutions/tests/SharpArch.xUnitTests/**/*.csproj");
		bool success = true;

		foreach(var testProj in testProjects) {
			try 
			{
				var projectPath = testProj.GetDirectory();
				var projectFilename = testProj.GetFilenameWithoutExtension();
//				var dotnetSettings = new DotNetCoreToolSettings {
//					WorkingDirectory = testProj.GetDirectory()
//				};
				var openCoverSettings = new OpenCoverSettings {
					OldStyle = true,
					ReturnTargetCodeOffset = 0,
					ArgumentCustomization = args => args.Append("-mergeoutput"),
					WorkingDirectory = projectPath,
				}
				.WithFilter("+[*]* -[*.Tests*]*")
				.ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
				.ExcludeByFile("*/*Designer.cs ");

				var testOutput = $"{artifactDirectory}/xunitTests-{projectFilename}.xml";
				Information("testOutput: {0}", MakeAbsolute(File(testOutput)));
				// todo: Detect NetCore framework version
				OpenCover(
					tool => tool.DotNetCoreTool(projectPath.ToString(),
						"xunit",
						$"-xml {testOutput} -c {buildConfig} --no-build --fx-version 2.0.7"),
					testCoverageOutputFile,
					openCoverSettings);
			}
			catch (Exception ex)
			{
				Error("Error running tests", ex);
				success = false;
			}
		}
    });


Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, artifactDirectory + "/CodeCoverageReport");
    });


Task("UploadTestResults")
    //.WithCriteria(() => !local)
    .Does(() => {
        CoverallsIo(testCoverageOutputFile);
        UploadFile("https://ci.appveyor.com/api/testresults/nunit3/"+appVeyorJobId, nunitTestResults);
		foreach(var xunitResults in GetFiles($"{artifactDirectory}/xunitTests-*.xml"))
		{
			UploadFile("https://ci.appveyor.com/api/testresults/xunit2/"+appVeyorJobId, xunitResults);
		}
    });


Task("RunUnitTests")
    .IsDependentOn("Build")
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


Task("CreateNugetPackages")
    .Does(() => {
        // copy templates to temp folder
        CopyDirectory(nugetTemplates, nugetTemp);
        // update templates
        ReplaceTextInFiles(nugetTemp+"/**/*.nuspec", "$(SemanticVersion)", nugetVersion);
        ReplaceTextInFiles(nugetTemp+"/**/*.nuspec", "$(NextMajorRelease)", nextMajorRelease);

        Func<string, string, string> removeBasePath = (path, basePath) => {
            var endOfBase = path.IndexOf(basePath);
            if (endOfBase < 0)
                return path; // base not found
            endOfBase += basePath.Length;
            return path.Substring(endOfBase);
        };

        Action<string> buildPackage = (string projectName) => {
            Information("Creating package {0}", projectName);
            var files = GetFiles($"{solutionsFolder}/{projectName}/bin/Release/**/{projectName}.*");
            var filePathes = files.Where(f=> !f.FullPath.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
                .Select(filePath => removeBasePath(filePath.FullPath, $"{projectName}/bin/Release/"));
            
            // create folders
            foreach(var frameworkLib in filePathes.Select(fp => new FilePath(fp).GetDirectory().FullPath).Distinct()) {
                CreateDirectory($"{nugetTemp}/{projectName}/lib/{frameworkLib}");
            };

            foreach (var file in filePathes) {
                var srcFile = $"{solutionsFolder}/{projectName}/bin/Release/{file}";
                var dstFile = $"{nugetTemp}/{projectName}/lib/{file}";
                CopyFile(srcFile, dstFile);
            };

            var exitCode = StartProcess("nuget.exe", new ProcessSettings {
                WorkingDirectory = $"{nugetTemp}/{projectName}",
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
    .Does(
        () => {}
    );
    

// EXECUTION
RunTarget(target);
