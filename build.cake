// DEFAULTS

#load "./.build/definitions.cake"


// ARGUMENTS

var target = Argument("target", "Default");

ProjectSettings settings = new ProjectSettings("sharparchitecture", "Sharp-Architecture", "SharpArch")
{
    CodeCoverage =
    {
        // Coverlet filter syntax: [Assembly]TypeName
        IncludeFilter = new List<string> { "[SharpArch.*]*" }
    }
};
// Exclude the test assemblies themselves. Coverlet does not instrument a test project's own
// assembly, but when a test project references another test assembly (e.g. SharpArch.XunitTests.NHibernate
// references SharpArch.XunitTests) coverlet lists that referenced test assembly's source files with 0
// hits because its tests do not run here, which surfaces as bogus 0% coverage on Coveralls.
settings.CodeCoverage.ExcludeFilter.AddRange(new[] { "[Suteki*]*", "[TransactionAttribute*]*", "[SharpArch.XunitTests*]*", "[SharpArch.Tests*]*" });

// SETUP / TEARDOWN

Setup<BuildInfo>(context =>
{
    var buildInfo = BuildInfo.Get(context, settings); // settings must be declared in main file

    Information("Building version {0} (tagged: {1}, local: {2}, release branch: {3})...", buildInfo.Version.NuGet,
        buildInfo.Repository.IsTagged, buildInfo.IsLocal, buildInfo.Repository.IsReleaseBranch);
    CreateDirectory(buildInfo.Paths.ArtifactsDir);
    CleanDirectory(buildInfo.Paths.ArtifactsDir);

    return buildInfo;
});

Teardown(context =>
{
    // Executed AFTER the last task.
});


// TASKS

#load "./.build/tasks.cake"


// EXECUTION

RunTarget(target);
