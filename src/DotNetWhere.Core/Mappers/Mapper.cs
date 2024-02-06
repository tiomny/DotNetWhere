using DotNetWhere.Core.Models;

using NuGet.LibraryModel;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace DotNetWhere.Core.Mappers;

internal static class Mapper
{
    public static PackageKey ToPackageKey(this PackageDependency packageDependency, string target) =>
        ToPackageKey(
            packageDependency.Id,
            packageDependency.VersionRange.MinVersion,
            target);


    public static PackageKey ToPackageKey(this LockFileTargetLibrary lockFileTargetLibrary, string target) =>
        ToPackageKey(
            lockFileTargetLibrary.Name!,
            lockFileTargetLibrary.Version,
            target);

    public static PackageKey ToPackageKey(this LibraryDependency libraryDependency, string target) =>
        ToPackageKey(
            libraryDependency.Name,
            libraryDependency.LibraryRange.VersionRange.MinVersion,
            target);

    private static PackageKey ToPackageKey(string name, NuGetVersion? version, string target) =>
        new PackageKey(
            name,
            (version ?? new NuGetVersion(0,0,0)).ToString(),
            target);
}
