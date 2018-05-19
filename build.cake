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
var solutionsFolder = "./Src";
var solutionFile = solutionsFolder + "/SharpArch.sln";
var nunitTestResults = artifactDirectory + "/Nunit3TestResults.xml";
var nugetTemplates = "./NugetTemplates";
var nugetTemp = artifactDirectory + "/Packages";
var codeCoverageReportDirectory = artifactDirectory + "/CodeCoverageReport";

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
		DotNetCoreRestore("Src");
	});


Task("Build")
	.IsDependentOn("SetVersion")
	.IsDependentOn("UpdateAppVeyorBuildNumber")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		DotNetCoreBuild("./Src/", new DotNetCoreBuildSettings {
			NoRestore = true,
			Configuration = buildConfig,
		});
	});


Task("RunNunitTests")
    .Does(() => 
    {
        var testAssemblies = GetFiles($"./Src/tests/SharpArch.Tests/bin/{buildConfig}/net462/SharpArch.Tests.dll")
            .Union(GetFiles($"./Src/tests/SharpArch.Tests.NHibernate/bin/{buildConfig}/net462/SharpArch.Tests.NHibernate.dll"))
            ;
        foreach (var item in testAssemblies)
        {
            Information("Test assembly: {0}", item);
        }

        Action<ICakeContext> testAction = tool => {
            tool.NUnit3(testAssemblies, 
                new NUnit3Settings {
                    OutputFile = artifactDirectory + "/TestOutput.xml",
                    ErrorOutputFile = artifactDirectory + "/ErrorOutput.xml",
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



Task("RunXunitTests")
	.Does((ctx) =>
	{

		var testProjects = GetFiles("./Src/tests/SharpArch.xUnitTests/**/*.csproj");
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
						$"-xml {testOutput} -c {buildConfig} --no-build "),
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

Task("CleanPreviousTestResults")
	.Does(() => 
	{
		if (FileExists(testCoverageOutputFile))
			DeleteFile(testCoverageOutputFile);
		DeleteFiles(artifactDirectory+ "/xunitTests-*.xml");
		if (DirectoryExists(codeCoverageReportDirectory))
			DeleteDirectory(codeCoverageReportDirectory, recursive: true);
	});

Task("GenerateCoverageReport")
	.WithCriteria(() => local)
	.Does(() =>
	{
		ReportGenerator(testCoverageOutputFile, codeCoverageReportDirectory);
	});


Task("UploadTestResults")
	.WithCriteria(() => !local)
	.Does(() => {
		CoverallsIo(testCoverageOutputFile);
		Information("Uploading nUnit result: {0}", nunitTestResults);
		UploadFile("https://ci.appveyor.com/api/testresults/nunit3/"+appVeyorJobId, nunitTestResults);
		foreach(var xunitResult in GetFiles($"{artifactDirectory}/xunitTests-*.xml"))
		{
			Information("Uploading xUnit results: {0}", xunitResult);
			UploadFile("https://ci.appveyor.com/api/testresults/xunit2/"+appVeyorJobId, xunitResult);
		}
	});


Task("RunUnitTests")
	//.IsDependentOn("Build")
	.IsDependentOn("CleanPreviousTestResults")
	.IsDependentOn("RunNunitTests")
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
			Profile = "./Src/SharpArch.sln.DotSettings",
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
