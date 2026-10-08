using DotNetWhere.Core.Models;

namespace DotNetWhere.Rendering;

internal sealed record SolutionOutput(string Name, IReadOnlyList<ProjectOutput> Projects)
{
    public static SolutionOutput From(Solution solution) => new(solution.Name,
        solution.Projects.Select(project => new ProjectOutput(project.Name, project.Version,
            project.Targets.Select(target => new TargetOutput(target.Version,
                target.Packages.Select(PackageOutput.From).ToArray())).ToArray())).ToArray());
}

internal sealed record ProjectOutput(string Name, string? Version, IReadOnlyList<TargetOutput> Targets);
internal sealed record TargetOutput(string Version, IReadOnlyList<PackageOutput> Packages);
internal sealed record PackageOutput(string Name, string Version, bool IsMatch, IReadOnlyList<PackageOutput>? Packages)
{
    public static PackageOutput From(Package package) => new(package.Name, package.Version, package.IsMatch,
        package.Packages?.Select(From).ToArray());
}
