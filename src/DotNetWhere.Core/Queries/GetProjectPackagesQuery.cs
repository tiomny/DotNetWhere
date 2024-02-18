using DotNetWhere.Core.Helpers;
using DotNetWhere.Core.Mappers;
using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Queries;

internal sealed class GetProjectPackagesQuery
(
    IPackageMatcher? packageMatcher,
    ITargetMatcher? targetMatcher,
    string restoreGraphOutputPath,
    string name
) : IResultHandler<Solution?>
{
    private readonly Dictionary<PackageKey, List<PackageKey>> _packages = [];
    private readonly Dictionary<PackageKey, Package> _matchPackages = [];

    public Result<Solution?> Handle()
    {
        var solution = GetSolution();

        return Result<Solution?>.Success(solution);
    }

    private Solution? GetSolution()
    {
        var dependencyGraphSpec = LockFileHelpers.GetDependencyGraphSpec(restoreGraphOutputPath);
        if (dependencyGraphSpec is null)
        {
            return null;
        }

        var projects = dependencyGraphSpec
            .Projects
            .Where(LockFileHelpers.IsPackage)
            .Select(GetProject)
            .WhereNotNull()
            .ToList();

        if (!projects.Any())
        {
            return null;
        }

        return new Solution(name, projects);
    }

    private Project? GetProject(PackageSpec project)
    {
        var lockFile = LockFileHelpers.GetLockFile(project.RestoreMetadata.OutputPath);
        if (lockFile is null)
        {
            return null;
        }

        CachePackages(lockFile);

        var targets =
            project
            .TargetFrameworks
            .Select(target => GetTargetNode(target, lockFile))
            .WhereNotNull()
            .ToList();

        if (!targets.Any())
        {
            return null;
        }

        return new Project(project.Name, project.Version.ToString(), targets);
    }

    private Target? GetTargetNode(
        TargetFrameworkInformation target,
        LockFile lockFile)
    {
        if (targetMatcher?.Match(target) == false)
        {
            return null;
        }

        var lockFileTarget = LockFileHelpers.GetLockFileTarget(lockFile, target.FrameworkName.ToString());
        if (lockFileTarget is null)
        {
            return null;
        }

        var packages = target
            .Dependencies
            .Select(dependency => GetPackageNode(dependency.ToPackageKey(target.ToString())))
            .WhereNotNull()
            .ToList();

        if (!packages.Any())
        {
            return null;
        }

        return new Target(target.ToString(), packages);
    }

    private Package? GetPackageNode(PackageKey packageKey)
    {
        if (_matchPackages.TryGetValue(packageKey, out var package))
        {
            return package;
        }

        var shouldAdd = false;

        var isMatch = false;

        if (packageMatcher is null)
        {
            shouldAdd = true;
        }
        else
        {
            shouldAdd = isMatch = packageMatcher.Match(packageKey);
        }

        var packages = new List<Package>();

        foreach (var dependentPackageKey in _packages[packageKey])
        {
            var dependentPackage = GetPackageNode(dependentPackageKey);
            if (dependentPackage is not null)
            {
                shouldAdd = true;
                packages.Add(dependentPackage);
            }
        }

        if (shouldAdd)
        {
            var ps = packages.Any() ? packages : null;
            var pkg = new Package(packageKey.Name, packageKey.Version, isMatch, ps);
            _matchPackages.Add(packageKey, pkg);
            return pkg;
        }

        return null;
    }

    private void CachePackages(LockFile lockFile)
    {
        foreach (var target in lockFile.Targets)
        {
            foreach (var library in target.Libraries)
            {
                if (!"package".Equals(library.Type)) continue;

                var package = library.ToPackageKey(target.Name);
                if (_packages.TryGetValue(package, out var packageDependencies))
                {
                    if (packageDependencies.Any()) continue;
                }
                else
                {
                    packageDependencies = [];
                    _packages.Add(package, packageDependencies);
                }

                foreach (var dependency in library.Dependencies)
                {
                    var dependentPackage = dependency.ToPackageKey(target.Name);
                    if (!_packages.ContainsKey(dependentPackage))
                    {
                        _packages[dependentPackage] = [];
                    }
                    packageDependencies.Add(dependentPackage);
                }
            }
        }
    }
}
