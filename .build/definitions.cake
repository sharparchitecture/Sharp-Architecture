// ADDINS
#addin nuget:?package=Cake.Coveralls&version=1.1.0
#addin nuget:?package=Cake.FileHelpers&version=7.0.0
#addin nuget:?package=Cake.AppVeyor&version=6.0.0

// TOOLS
#tool nuget:?package=GitReleaseManager&version=0.17.0
#tool nuget:?package=GitVersion.CommandLine&version=5.12.0
#tool nuget:?package=OpenCover&version=4.7.1221
#tool nuget:?package=ReportGenerator&version=5.2.2


public class CodeCoverageSettings
{
    public string ExcludeByFile { get; set; } = "*/*Designer.cs";
    public string ExcludeByAttribute { get; set; } = "*.ExcludeFromCodeCoverage*";
    public string ExcludeFilter { get; set; } = "-[Tests*]*;-[*]Microsoft.CodeAnalysis*;-[*]System.Runtime.CompilerServices.*";
    public string IncludeFilter { get; set; }
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
            IncludeFilter = $"+[solutionName*]*"
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
    public string TestCoverageOutputFile { get; set; }
    public string TestCoverageReportDir { get; set; }
    public string PackagesDir { get; set; }
    public string BuildPropsFile { get; set; }
    public string TestsRootDir { get; set; }
    public string SamplesRootDir { get; set; }
    public string CommonAssemblyVersionFile { get; set; }

    public Paths(ICakeContext context)
    {
        RootDir = context.MakeAbsolute(context.Directory("./"));
        SrcDir = RootDir.Combine("src").ToString();
        ArtifactsDir = RootDir.Combine("artifacts").ToString();
        TestCoverageOutputFile = ArtifactsDir + "/OpenCover.xml";
        TestCoverageReportDir = ArtifactsDir + "/CodeCoverageReport";
        PackagesDir = ArtifactsDir + "/packages";
        BuildPropsFile = SrcDir + "/Directory.Build.props";
        TestsRootDir = SrcDir + "/tests";
        CommonAssemblyVersionFile = SrcDir + "/common/AssemblyVersion.cs";
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

        // Calculate version and commit hash
        GitVersion semVersion = context.GitVersion();
        var version = new BuildVersion(
            semVersion.NuGetVersion,
            semVersion.FullBuildMetaData,
            semVersion.InformationalVersion,
            $"{semVersion.Major+1}.0.0",
            semVersion.Sha,
            semVersion.MajorMinorPatch
        );

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


