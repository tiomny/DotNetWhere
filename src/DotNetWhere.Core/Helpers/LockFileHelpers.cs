global using NuGet.Common;
global using NuGet.ProjectModel;

namespace DotNetWhere.Core.Helpers;

internal static class LockFileHelpers
{
    private const string LockFileName = "project.assets.json";

    public static DependencyGraphSpec GetDependencyGraphSpec(string path) =>
    DependencyGraphSpec.Load(path);

    public static LockFile GetLockFile(string path) =>
        LockFileUtilities.GetLockFile(Path.Combine(path, LockFileName), NullLogger.Instance);

    public static LockFileTarget? GetLockFileTarget(
        LockFile lockFile,
        string targetName) =>
        lockFile
            .Targets
            .FirstOrDefault(target => target.TargetFramework.ToString().Equals(targetName));

    public static LockFileTargetLibrary? GetLockFileTargetLibrary(
        LockFileTarget lockFileTarget,
        string libraryName) =>
        lockFileTarget
            .Libraries
            .FirstOrDefault(library => libraryName.Equals(library.Name));

    public static bool IsPackage(PackageSpec project) =>
        project.RestoreMetadata.ProjectStyle is ProjectStyle.PackageReference;
}
