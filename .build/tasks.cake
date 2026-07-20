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

        // Coverlet's MSBuild integration runs per test project. Testing the whole solution in one call makes
        // every project write to the same CoverletOutputDirectory with the same CoverletOutputName, so each TFM
        // slot is overwritten by the *last* project that runs and only that project's coverage survives. Instead,
        // test each project individually with a unique CoverletOutputName; Coverlet still appends the TFM for
        // multi-targeted projects, producing files like coverage.<Project>.<TFM>.cobertura.xml. UploadCoverage
        // then sends each file to Coveralls as a distinct parallel job so the full SharpArch.* surface is covered
        // instead of just the last project's slice.
        var testProjects = GetFiles($"{build.Paths.SrcDir}/**/*.csproj")
            .Where(f => f.GetFilenameWithoutExtension().ToString().Contains("Tests"))
            .OrderBy(f => f.GetFilenameWithoutExtension().ToString())
            .ToList();

        foreach (var project in testProjects)
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();
            var coverletSettings = new CoverletSettings
            {
                CollectCoverage    = true,
                CoverletOutputFormat = CoverletOutputFormat.cobertura,
                CoverletOutputDirectory = new DirectoryPath(build.Paths.ArtifactsDir),
                CoverletOutputName = $"coverage.{projectName}",
                ExcludeByFile      = new List<string> { build.Settings.CodeCoverage.ExcludeByFile },
                ExcludeByAttribute = new List<string> { build.Settings.CodeCoverage.ExcludeByAttribute },
                Include            = build.Settings.CodeCoverage.IncludeFilter,
                Exclude            = build.Settings.CodeCoverage.ExcludeFilter,
            };

            Information("Calculating code coverage for {0} ...", projectName);
            DotNetTest(
                project.FullPath,
                BuildTestSettings("Debug", projectName),
                coverletSettings
            );
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
        DeleteFiles(build.Paths.ArtifactsDir + "/coverage.*.cobertura.xml");
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
    .WithCriteria<BuildInfo>((ctx, build) => build.IsPullRequest == false && build.IsLocal == false)
    .Does<BuildInfo>(build =>
    {
        // Uses the self-contained coveralls-windows.exe "coverage-reporter" binary (no .NET runtime
        // dependency) instead of the abandoned, .NET-6-only coveralls.net/Cake.Coveralls tool.
        //
        // Each per-project/per-TFM Cobertura file is uploaded as a DISTINCT parallel job. The
        // coverage-reporter does NOT merge overlapping coverage within a single job (its
        // SourceFiles.add just concatenates source-file entries), so a single multi-file `report` —
        // or several `report` calls sharing the auto-detected AppVeyor service_job_id — would
        // concatenate and lose data. Distinct --job-id per upload makes Coveralls treat each file as
        // a separate parallel job; the final `coveralls done` fires the completion webhook that
        // aggregates all jobs for this build (service_number, auto-detected from AppVeyor) with
        // server-side per-line-max merging. `done` MUST run even when individual uploads fail,
        // otherwise Coveralls never finalizes the merge — hence the try/finally.
        var reporterExe = EnvironmentVariable("COVERALLS_REPORTER_EXE") ?? "coveralls-windows.exe";
        var buildNumber = EnvironmentVariable("APPVEYOR_BUILD_NUMBER");

        var coverageFiles = GetFiles(build.Paths.TestCoverageGlobPattern)
            .OrderBy(f => f.GetFilename().ToString())
            .ToList();

        if (coverageFiles.Count == 0)
        {
            Warning("No coverage files found matching {0}; skipping Coveralls upload.", build.Paths.TestCoverageGlobPattern);
            return;
        }

        var failures = new List<string>();
        var uploaded = 0;
        try
        {
            foreach (var file in coverageFiles)
            {
                // Distinct job id per project+TFM (prefixed with the build number for cross-build
                // uniqueness) so each upload is a separate parallel job rather than overwriting the
                // auto-detected AppVeyor service_job_id that all calls in this job share.
                var jobId = string.IsNullOrEmpty(buildNumber)
                    ? file.GetFilenameWithoutExtension().ToString()
                    : $"{buildNumber}-{file.GetFilenameWithoutExtension()}";

                Information("Uploading coverage {0} to Coveralls (parallel job {1}) ...", file.GetFilename(), jobId);

                var exitCode = StartProcess(reporterExe, new ProcessSettings
                {
                    Arguments = new ProcessArgumentBuilder()
                        .Append("report")
                        .AppendQuoted(file.FullPath)
                        .Append("--format=cobertura")
                        .Append("--parallel")
                        .Append($"--job-id={jobId}")
                        .Append("--no-logo")
                });

                if (exitCode != 0)
                    failures.Add($"{file.GetFilename()} (exit code {exitCode})");
                else
                    uploaded++;
            }
        }
        finally
        {
            // Fire the completion webhook so Coveralls aggregates the parallel jobs for this build,
            // regardless of whether some uploads failed. Only finalize if at least one job was sent.
            if (uploaded > 0)
            {
                Information("Finalizing Coveralls parallel build ...");
                var doneExitCode = StartProcess(reporterExe, new ProcessSettings
                {
                    Arguments = new ProcessArgumentBuilder()
                        .Append("done")
                        .Append("--no-logo")
                });
                if (doneExitCode != 0)
                    failures.Add($"coveralls done (exit code {doneExitCode})");
            }
        }

        if (failures.Count > 0)
            throw new Exception("Coveralls upload reported failures:\n" + string.Join("\n", failures));
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
