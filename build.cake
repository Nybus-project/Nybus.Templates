#tool "nuget:?package=NuGet.CommandLine&version=4.9.2"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#addin "Cake.FileHelpers"

#load "./build/types.cake"

var target = Argument("Target", "Full");

Setup<BuildState>(_ => 
{
    var state = new BuildState
    {
        Paths = new BuildPaths
        {
            SolutionFile = MakeAbsolute(File("./Nybus.Templates.sln")),
            NuSpecFile = MakeAbsolute(File("./Nybus.Templates.nuspec"))
        }
    };

    CleanDirectory(state.Paths.OutputFolder);

    return state;
});

Task("Version")
    .Does<BuildState>(state =>
{
    var version = GitVersion();

    var packageVersion = version.SemVer;
    var buildVersion = $"{version.FullSemVer}+{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

    state.Version = new VersionInfo
    {
        PackageVersion = packageVersion,
        BuildVersion = buildVersion
    };

    Information($"Package version: {state.Version.PackageVersion}");
    Information($"Build version: {state.Version.BuildVersion}");

    if (BuildSystem.IsRunningOnAppVeyor)
    {
        AppVeyor.UpdateBuildVersion(state.Version.BuildVersion);
    }
});

Task("UpdateTemplatesToLatestVersion")
    .Does(() => 
{
    var packages = NuGetList("Nybus.Abstractions");

    var latestVersion = packages.FirstOrDefault().Version;

    Information($"Latest version for Nybus.Abstractions: {latestVersion}");

    var files = ReplaceRegexInFiles("./content/**/*.csproj", 
                        @"<PackageReference Include=""(Nybus[\w\.]*)"" Version="".+"" />", 
                        $@"<PackageReference Include=""$1"" Version=""{latestVersion}"" />");

    Information($"Updated {files.Count()} files");
    foreach (var file in files)
    {
        Information($"\tUpdated {file.FullPath}");
    }
});

Task("Pack")
    .IsDependentOn("Version")
    .Does<BuildState>(state => 
{
    var settings = new NuGetPackSettings
    {
        Version = state.Version.PackageVersion,
        OutputDirectory = state.Paths.OutputFolder
    };

    NuGetPack(state.Paths.NuSpecFile, settings);
});

Task("UploadPackagesToAppVeyor")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsRunningOnAppVeyor)
    .Does<BuildState>(state => 
{
    Information("Uploading packages");
    var files = GetFiles($"{state.Paths.OutputFolder}/*.nukpg");

    foreach (var file in files)
    {
        Information($"\tUploading {file.GetFilename()}");
        AppVeyor.UploadArtifact(file, new AppVeyorUploadArtifactsSettings {
            ArtifactType = AppVeyorUploadArtifactType.NuGetPackage,
            DeploymentName = "NuGet"
        });
    }
});

Task("Push")
    .IsDependentOn("UploadPackagesToAppVeyor");

Task("Full")
    .IsDependentOn("Version")
    .IsDependentOn("UpdateTemplatesToLatestVersion")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

RunTarget(target);