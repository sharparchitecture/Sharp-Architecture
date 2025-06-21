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
        var projectPath = build.Paths.SrcDir;
        var projectFilename = build.Settings.SolutionName;
        // keep in sync with src/Directory.Build.props
        var solutionFullPath = new DirectoryPath(build.Paths.SrcDir).Combine(build.Settings.SolutionName) + ".sln";
        {
            Func<string,ProcessArgumentBuilder> buildProcessArgs = (buildCfg) => {
                var pb = new ProcessArgumentBuilder()
                    .AppendSwitch("--configuration", buildCfg)
                    .AppendSwitch("--filter", "Category!=ManualTests")
                    .AppendSwitch("--results-directory", build.Paths.ArtifactsDir)
                    .Append("--no-restore")
                    .Append("--no-build");
                if (!build.IsLocal) {
                    pb.AppendSwitch("--test-adapter-path", ".")
                        .AppendSwitch("--logger", "AppVeyor");
                }
                else {
                    pb.AppendSwitch("--logger", $"trx;LogFileName={projectFilename}.trx");
                }
                return pb;
            };

            Information("Calculating code coverage for {0} ...", projectFilename);

            var openCoverSettings = new OpenCoverSettings
            {
                OldStyle = true,
                ReturnTargetCodeOffset = 0,
                ArgumentCustomization = args => args.Append("-mergeoutput").Append("-hideskipped:File;Filter;Attribute"),
                WorkingDirectory = projectPath,
            }
            .WithFilter($"{build.Settings.CodeCoverage.IncludeFilter} {build.Settings.CodeCoverage.ExcludeFilter}")
            .ExcludeByAttribute(build.Settings.CodeCoverage.ExcludeByAttribute)
            .ExcludeByFile(build.Settings.CodeCoverage.ExcludeByFile);

            // run open cover for debug build configuration
            OpenCover(
                tool => tool.DotNetTool(
                    projectPath.ToString(),
                    "test",
                    buildProcessArgs("Debug")
                ),
                build.Paths.TestCoverageOutputFile,
                openCoverSettings
            );

            // run tests again if Release mode was requested
            if (build.IsRelease)
            {
                Information("Running Release mode tests for {0} ...", projectFilename);
                DotNetTool(
                    solutionFullPath,
                    "test",
                    buildProcessArgs("Release")
                );
            }
        }
    })
    .DeferOnError();

Task("CleanPreviousTestResults")
    .Does<BuildInfo>(build =>
    {
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
        ReportGenerator((FilePath)build.Paths.TestCoverageOutputFile, build.Paths.TestCoverageReportDir);
    });

Task("UploadCoverage")
    .WithCriteria<BuildInfo>((ctx, build) => !build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        CoverallsNet(build.Paths.TestCoverageOutputFile, CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
        {
            RepoTokenVariable = "COVERALLS_REPO_TOKEN"
        });
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
