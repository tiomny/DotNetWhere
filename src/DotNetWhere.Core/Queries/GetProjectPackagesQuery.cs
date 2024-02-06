using DotNetWhere.Core.Helpers;
using DotNetWhere.Core.Mappers;
using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Queries;

internal sealed class GetProjectPackagesQuery
(
    IMatcher matcher,
    string restoreGraphOutputPath,
    string name
) : IResultHandler<Solution>
{
    private readonly Dictionary<PackageKey, List<PackageKey>> _packages = new();
    private readonly Dictionary<PackageKey, Package> _matchPackages = new();

    public Result<Solution> Handle()
    {
        var solution = GetSolution();

        return Result<Solution>.Success(solution);
    }

    private Solution GetSolution()
    {
        var node = new Solution(name);

        var dependencyGraphSpec = LockFileHelpers.GetDependencyGraphSpec(restoreGraphOutputPath);
        if (dependencyGraphSpec is null) return node;

        node.Projects.AddRange(
            dependencyGraphSpec
            .Projects
            .Where(LockFileHelpers.IsPackage)
            .Select(GetProject)
            );

        return node;
    }

    private Project GetProject(PackageSpec project)
    {
        var projectNode = new Project(project.Name, null);

        var lockFile = LockFileHelpers.GetLockFile(project.RestoreMetadata.OutputPath);
        if (lockFile is null) return projectNode;

        CachePackages(lockFile);

        projectNode.Targets.AddRange(
            project
            .TargetFrameworks
            .Select(target => GetTargetNode(target, lockFile)));

        return projectNode;
    }

    private Target GetTargetNode(
        TargetFrameworkInformation target,
        LockFile lockFile)
    {
        var targetNode = new Target(target.ToString());

        var lockFileTarget = LockFileHelpers.GetLockFileTarget(lockFile, target.FrameworkName.ToString());
        if (lockFileTarget is null) return targetNode;


        targetNode.Packages.AddRange(
        target
            .Dependencies
            .Select(dependency => GetPackageNode(dependency.ToPackageKey(target.ToString())))
            .Where(dependency => dependency is not null)
            );

        return targetNode;
    }

    private Package? GetPackageNode(PackageKey packageKey)
    {
        if (_matchPackages.TryGetValue(packageKey, out var package))
        {
            return package;
        }

        var isMatch = matcher.Match(packageKey.Name);
        var transitiveMatch = isMatch;
        package = new Package(packageKey.Name, packageKey.Version, isMatch);

        foreach (var dependentPackageKey in _packages[packageKey])
        {
            var dependentPackage = GetPackageNode(dependentPackageKey);
            if (dependentPackage is not null)
            {
                transitiveMatch = true;
                package.Packages.Add(dependentPackage);
            }
        }

        if (transitiveMatch)
        {
            _matchPackages.Add(packageKey, package);
            return package;
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
