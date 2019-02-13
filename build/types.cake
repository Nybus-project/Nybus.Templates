public class BuildState
{
    public VersionInfo Version { get; set; }

    public BuildPaths Paths { get; set; }
}

public class VersionInfo
{
    public string PackageVersion { get; set; }

    public string BuildVersion {get; set; }
}

public class BuildPaths
{
    public FilePath SolutionFile { get; set; }

    public FilePath NuSpecFile { get; set; }

    public DirectoryPath SolutionFolder => SolutionFile.GetDirectory();

    public DirectoryPath OutputFolder => SolutionFolder.Combine("outputs");
}