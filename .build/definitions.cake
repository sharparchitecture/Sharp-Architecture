// ADDINS
#addin nuget:?package=Cake.Coveralls&version=6.0.0
#addin nuget:?package=Cake.Coverlet&version=6.0.1
#addin nuget:?package=Cake.FileHelpers&version=9.0.0
#addin nuget:?package=Cake.AppVeyor&version=10.0.0

// TOOLS
#tool nuget:?package=GitReleaseManager.Tool
#tool "dotnet:?package=GitVersion.Tool&version=6.0.0"
#tool nuget:?package=ReportGenerator&version=5.4.8


public class CodeCoverageSettings
{
    /// <summary>
    /// Glob pattern to exclude source files from coverage (Coverlet format, e.g. "**/*Designer.cs").
    /// </summary>
    public string ExcludeByFile { get; set; } = "**/*Designer.cs";

    /// <summary>
    /// Attribute short name used to exclude members from coverage (e.g. "ExcludeFromCodeCoverage").
    /// </summary>
    public string ExcludeByAttribute { get; set; } = "ExcludeFromCodeCoverage";

    /// <summary>
    /// Coverlet exclude filters in [Assembly]Type format, e.g. "[Tests*]*".
    /// </summary>
    public List<string> ExcludeFilter { get; set; } = new List<string> { "[Tests*]*", "[*]Microsoft.CodeAnalysis*", "[*]System.Runtime.CompilerServices.*" };

    /// <summary>
    /// Coverlet include filters in [Assembly]Type format, e.g. "[SharpArch.*]*".
    /// </summary>
    public List<string> IncludeFilter { get; set; } = new List<string>();
}

// params
public class ProjectSettings {
    public string RepoOwner { get; set; }
    public string RepoName { get; set; }
    public string SolutionName { get; set; }

    public CodeCoverageSettings CodeCoverage { get; }

    public ProjectSettings(string repoOwner, string repoName, string solutionName)
    {
        if (string.IsNullOrEmpty(repoOwner))
            throw new ArgumentNullException(nameof(repoOwner), "Value cannot be null or empty.");
        if (string.IsNullOrEmpty(repoName))
            throw new ArgumentNullException(nameof(repoName), "Value cannot be null or empty.");
        if (string.IsNullOrEmpty(solutionName))
            throw new ArgumentNullException(nameof(solutionName), "Value cannot be null or empty.");

        RepoOwner = repoOwner;
        RepoName = repoName;
        SolutionName = solutionName;

        CodeCoverage = new CodeCoverageSettings {
            IncludeFilter = new List<string> { $"[{solutionName}*]*" }
        };
    }
}

public class Credentials {
    public string UserName { get; }
    public string Password { get; }

    public Credentials(string userName, string password) {
        UserName = userName;
        Password = password;
    }
}

public class BuildVersion {
    public string NuGet { get; }
    public string Full { get; }
    public string Informational { get; }
    public string NextMajor { get; }
    public string CommitHash { get; }
    public string Milestone { get; }

    public BuildVersion(string nuget, string full, string informational, string nextMajor, string commitHash, string milestone) {
        NuGet = nuget;
        Full = full;
        Informational = informational;
        NextMajor = nextMajor;
        CommitHash = commitHash;
        Milestone = milestone;
    }
}

public class RepositoryInfo {
    public bool IsPullRequest { get; protected set; }
    public bool IsMain { get; protected set; }
    public bool IsDevelopBranch { get; protected set; }
    // Release or hotfix branch
    public bool IsReleaseBranch { get; protected set; }
    public bool IsTagged { get; protected set; }

    public static RepositoryInfo Get(BuildSystem buildSystem, ProjectSettings settings) {
        return new RepositoryInfo {
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsReleaseBranch = buildSystem.AppVeyor.Environment.Repository.Branch.IndexOf("release/", StringComparison.OrdinalIgnoreCase) >= 0
                || buildSystem.AppVeyor.Environment.Repository.Branch.IndexOf("releases/", StringComparison.OrdinalIgnoreCase) >= 0
                || buildSystem.AppVeyor.Environment.Repository.Branch.IndexOf("hotfix/", StringComparison.OrdinalIgnoreCase) >= 0
                || buildSystem.AppVeyor.Environment.Repository.Branch.IndexOf("hotfixes/", StringComparison.OrdinalIgnoreCase) >= 0
                ,
            IsTagged = buildSystem.AppVeyor.Environment.Repository.Tag.IsTag,
            IsMain = StringComparer.OrdinalIgnoreCase.Equals($"{settings.RepoOwner}/{settings.RepoName}", buildSystem.AppVeyor.Environment.Repository.Name),
        };
    }
}

// default paths and files
public class Paths {
    public DirectoryPath RootDir { get; }
    public string SrcDir { get; set; }
    public string ArtifactsDir { get; set; }
    /// <summary>Glob matching the per-TargetFramework OpenCover XML files Coverlet writes (e.g. coverage.net10.0.opencover.xml). Consumed by ReportGenerator, which can merge multiple inputs into one report.</summary>
    public string TestCoverageGlobPattern { get; set; }
    /// <summary>Raw per-TFM OpenCover XML Coverlet writes for the primary framework; copied to <see cref="TestCoverageOutputFile"/> since Coverlet always suffixes multi-targeted output with the TFM.</summary>
    public string PrimaryCoverageSourceFile { get; set; }
    /// <summary>Flat (no-TFM) OpenCover XML used for Coveralls upload (CoverallsNet has no multi-file/glob overload).</summary>
    public string TestCoverageOutputFile { get; set; }
    public string TestCoverageReportDir { get; set; }
    public string PackagesDir { get; set; }
    public string BuildPropsFile { get; set; }
    public string CommonAssemblyVersionFile { get; set; }

    public Paths(ICakeContext context)
    {
        RootDir = context.MakeAbsolute(context.Directory("./"));
        SrcDir = RootDir.Combine("Src").ToString();
        ArtifactsDir = RootDir.Combine("artifacts").ToString();
        TestCoverageGlobPattern = ArtifactsDir + "/coverage.*.opencover.xml";
        PrimaryCoverageSourceFile = ArtifactsDir + "/coverage.net10.0.opencover.xml";
        TestCoverageOutputFile = ArtifactsDir + "/coverage.opencover.xml";
        TestCoverageReportDir = ArtifactsDir + "/CodeCoverageReport";
        PackagesDir = ArtifactsDir + "/packages";
        BuildPropsFile = SrcDir + "/Directory.Build.props";
        CommonAssemblyVersionFile = SrcDir + "/Common/AssemblyVersion.cs";
    }

}


public class BuildInfo {
    public string Target { get; protected set; }
    public string Config { get; protected set; }

    public bool IsDebug { get; protected set; }
    public bool IsRelease {get; protected set;}

    public bool IsLocal { get; protected set; }
    public string AppVeyorJobId { get; protected set; }

    public BuildVersion Version { get; protected set; }

    public RepositoryInfo Repository { get; protected set; }

    public string  GitHubToken { get; protected set; }

    public Paths Paths { get; protected set; }

    public ProjectSettings Settings { get; protected set; }

    public static BuildInfo Get(ICakeContext context, ProjectSettings settings)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        var target = context.Argument("target", "Default");
        var config = context.Argument("buildConfig", "Release");

        var buildSystem = context.BuildSystem();
        var repositoryInfo = RepositoryInfo.Get(buildSystem, settings);
        BuildVersion version;

        if (repositoryInfo.IsPullRequest) {
            // GitVersion fails on PR builds, use 0.PullRequestId.BuildNumber as a version number
            var buildVersion = $"0.{buildSystem.AppVeyor.Environment.PullRequest.Number}.{buildSystem.AppVeyor.Environment.Build.Number}";
            var commitHash = buildSystem.AppVeyor.Environment.Repository.Commit.Id;

            version = new BuildVersion(
                buildVersion,
                $"{buildVersion}/{commitHash}-PR-{buildSystem.AppVeyor.Environment.PullRequest.Title}",
                buildVersion,
                $"{buildVersion}.0",
                commitHash,
                buildVersion
            );
        }
        else {
            // Calculate version and commit hash
            GitVersion semVersion = context.GitVersion();
            version = new BuildVersion(
                semVersion.NuGetVersion,
                semVersion.FullBuildMetaData,
                semVersion.InformationalVersion,
                $"{semVersion.Major+1}.0.0",
                semVersion.Sha,
                semVersion.MajorMinorPatch
            );
        }

        var gitHubToken = context.EnvironmentVariable("GITHUB_TOKEN");

        return new BuildInfo {
            Target = target,
            Config = config,
            IsDebug = string.Equals(config, "Debug", StringComparison.OrdinalIgnoreCase),
            IsRelease = string.Equals(config, "Release", StringComparison.OrdinalIgnoreCase),
            IsLocal = buildSystem.IsLocalBuild,
            AppVeyorJobId = buildSystem.AppVeyor.Environment.JobId,
            Version = version,
            Repository = RepositoryInfo.Get(buildSystem, settings),
            GitHubToken = gitHubToken,
            Settings = settings,
            Paths = new Paths(context),
        };
    }
}
