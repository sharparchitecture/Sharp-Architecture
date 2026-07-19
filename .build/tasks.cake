// TASKS

Task("CleanAll")
    .Does<BuildInfo>(build =>
    {
        CleanDirectories($"{build.Paths.SrcDir}/**/obj");
        CleanDirectories($"{build.Paths.SrcDir}/**/bin");
        CleanDirectories($"{build.Paths.ArtifactsDir}/**");
    });

Task("SetVersion")
    .Does<BuildInfo>(build =>
    {
        CreateAssemblyInfo(build.Paths.CommonAssemblyVersionFile, new AssemblyInfoSettings
        {
            FileVersion = build.Version.Milestone,
            InformationalVersion = build.Version.Informational,
            Version = build.Version.Milestone
        });
    });


Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .ContinueOnError()
    .Does<BuildInfo>(build =>
    {
        AppVeyor.UpdateBuildVersion(build.Version.Full);
    });


Task("Restore")
    .Does<BuildInfo>(build =>
    {
        DotNetRestore(build.Paths.SrcDir);
    });


Task("RunXunitTests")
    .Does<BuildInfo>(build =>
    {
        var solutionFullPath = new DirectoryPath(build.Paths.SrcDir).Combine(build.Settings.SolutionName) + ".sln";

        // Build DotNetTestSettings for a given configuration and log-file name.
        DotNetTestSettings BuildTestSettings(string buildCfg, string logFilename)
        {
            var ts = new DotNetTestSettings
            {
                Configuration = buildCfg,
                Filter        = "Category!=ManualTests",
                ResultsDirectory = new DirectoryPath(build.Paths.ArtifactsDir),
                NoRestore = true,
                NoBuild   = true,
                ArgumentCustomization = args =>
                {
                    if (!build.IsLocal)
                        return args.AppendSwitch("--test-adapter-path", ".")
                                   .AppendSwitch("--logger", "AppVeyor");
                    return args.AppendSwitch("--logger", $"trx;LogFileName={logFilename}.trx");
                }
            };
            return ts;
        }

        var coverletSettings = new CoverletSettings
        {
            CollectCoverage    = true,
            CoverletOutputFormat = CoverletOutputFormat.cobertura | CoverletOutputFormat.json,
            CoverletOutputDirectory = new DirectoryPath(build.Paths.ArtifactsDir),
            CoverletOutputName = "coverage",
            ExcludeByFile      = new List<string> { build.Settings.CodeCoverage.ExcludeByFile },
            ExcludeByAttribute = new List<string> { build.Settings.CodeCoverage.ExcludeByAttribute },
            Include            = build.Settings.CodeCoverage.IncludeFilter,
            Exclude            = build.Settings.CodeCoverage.ExcludeFilter,
        };

        Information("Calculating code coverage for {0} ...", build.Settings.SolutionName);

        try
        {
            DotNetTest(
                solutionFullPath,
                BuildTestSettings("Debug", build.Settings.SolutionName),
                coverletSettings
            );
        }
        finally
        {
            // DotNetTest throws on test failures, which would otherwise skip this step.
            // Coverlet always suffixes multi-targeted output with the TFM; copy the primary
            // framework's report to the flat name consumed by the Coveralls upload.
            if (FileExists(build.Paths.PrimaryCoverageSourceFile))
                CopyFile(build.Paths.PrimaryCoverageSourceFile, build.Paths.TestCoverageOutputFile);
        }

        // Run Release-mode tests (no coverage) when a Release build was requested.
        if (build.IsRelease)
        {
            Information("Running Release mode tests for {0} ...", build.Settings.SolutionName);
            DotNetTest(solutionFullPath, BuildTestSettings("Release", build.Settings.SolutionName));
        }
    })
    .DeferOnError();

Task("CleanPreviousTestResults")
    .Does<BuildInfo>(build =>
    {
        DeleteFiles(build.Paths.ArtifactsDir + "/coverage.*.json");
        DeleteFiles(build.Paths.ArtifactsDir + "/coverage.*.cobertura.xml");
        if (FileExists(build.Paths.TestCoverageOutputFile))
            DeleteFile(build.Paths.TestCoverageOutputFile);
        DeleteFiles(build.Paths.ArtifactsDir + "/*.trx");
        if (DirectoryExists(build.Paths.TestCoverageReportDir))
            DeleteDirectory(build.Paths.TestCoverageReportDir, new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
    });

Task("GenerateCoverageReport")
    .WithCriteria<BuildInfo>((ctx, build) => build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        ReportGenerator(new GlobPattern(build.Paths.TestCoverageGlobPattern), build.Paths.TestCoverageReportDir);
    });

Task("UploadCoverage")
    .WithCriteria<BuildInfo>((ctx, build) => !build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        // Uses the self-contained coveralls-windows.exe "coverage-reporter" binary (no .NET runtime
        // dependency) instead of the abandoned, .NET-6-only coveralls.net/Cake.Coveralls tool.
        var reporterExe = EnvironmentVariable("COVERALLS_REPORTER_EXE") ?? "coveralls-windows.exe";
        var repoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN");

        var exitCode = StartProcess(reporterExe, new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("report")
                .AppendQuoted(build.Paths.TestCoverageOutputFile)
                .Append("--format=cobertura")
                .AppendSwitchSecret("--repo-token", "=", repoToken)
        });

        if (exitCode != 0)
            throw new Exception($"Coveralls report upload failed with exit code {exitCode}.");
    });

Task("RunUnitTests")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    .IsDependentOn("RunXunitTests")
    .IsDependentOn("GenerateCoverageReport")
    .IsDependentOn("UploadCoverage")
    .Does<BuildInfo>(build =>
    {
        Information("Done Test");
    });

Task("UpdateReleaseNotesLink")
    .WithCriteria<BuildInfo>((ctx, build) => build.Repository.IsTagged)
    .Does<BuildInfo>(build =>
    {
        var releaseNotesUrl = $"https://github.com/{build.Settings.RepoOwner}/{build.Settings.RepoName}/releases/tag/{build.Version.Milestone}";
        Information("Updating ReleaseNotes URL to '{0}'", releaseNotesUrl);
        XmlPoke(build.Paths.BuildPropsFile,
            "/Project/PropertyGroup[@Label=\"Package\"]/PackageReleaseNotes",
            releaseNotesUrl
        );
    });


Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("UpdateReleaseNotesLink")
    .IsDependentOn("Restore")
    .Does<BuildInfo>(build =>
    {
        if (build.IsRelease) {
            Information("Running {0} build to calculate code coverage", "Debug");
            // need Debug build for code coverage
            DotNetBuild(build.Paths.SrcDir, new DotNetBuildSettings {
                NoRestore = true,
                Configuration = "Debug",
            });
        }
        Information("Running {0} build", build.Config);
        DotNetBuild(build.Paths.SrcDir, new DotNetBuildSettings {
            NoRestore = true,
            Configuration = build.Config,
        });
    });


Task("CreateNugetPackages")
    .Does<BuildInfo>(build =>
    {
        DotNetPack(build.Paths.SrcDir, new DotNetPackSettings {
            Configuration = build.Config,
            NoRestore = true,
            NoBuild = true,
            ArgumentCustomization = args =>
                args.Append($"-p:Version={build.Version.NuGet}")
                    .Append($"-p:PackageOutputPath={build.Paths.PackagesDir}")
        });
    });

Task("CreateRelease")
    .WithCriteria<BuildInfo>((ctx, build) =>
        build.Repository.IsMain && build.Repository.IsReleaseBranch && build.Repository.IsPullRequest == false)
    .Does<BuildInfo>(build =>
    {
        GitReleaseManagerCreate(
            build.GitHubToken,
            build.Settings.RepoOwner, build.Settings.RepoName,
            new GitReleaseManagerCreateSettings {
              Milestone = build.Version.Milestone,
              TargetCommitish = "master"
        });
    });

Task("CloseMilestone")
    .WithCriteria<BuildInfo>((ctx, build) =>
        build.Repository.IsMain && build.Repository.IsTagged && build.Repository.IsPullRequest == false)
    .Does<BuildInfo>(build =>
    {
        GitReleaseManagerClose(
            build.GitHubToken,
            build.Settings.RepoOwner, build.Settings.RepoName,
            build.Version.Milestone
        );
    });

Task("Default")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("CreateNugetPackages")
    .IsDependentOn("CreateRelease")
    .IsDependentOn("CloseMilestone")
    .Does(
        () => {}
    );
