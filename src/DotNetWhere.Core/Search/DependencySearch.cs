using DotNetWhere.Core.Graph;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Search;

public static class DependencySearch
{
    public static Solution? Find(
        DependencyGraph graph,
        SearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var projects = new List<Project>();
        foreach (var project in graph.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targets = new List<Target>();
            foreach (var target in project.Targets)
            {
                if (!criteria.MatchesTarget(target.Framework)) continue;

                var cache = new Dictionary<string, Package?>(StringComparer.OrdinalIgnoreCase);
                var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var packages = target.Roots
                    .Select(Visit)
                    .OfType<Package>()
                    .ToList();

                if (packages.Count > 0) targets.Add(new Target(target.Framework, packages));

                Package? Visit(string name)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (cache.TryGetValue(name, out var cached)) return cached;
                    if (!target.Packages.TryGetValue(name, out var package))
                        throw new InvalidGraphException(
                            $"Resolved package '{name}' is missing in '{target.Framework}'.");
                    if (!visiting.Add(name))
                        throw new InvalidGraphException($"Dependency cycle at '{name}' in '{target.Framework}'.");

                    var children = package.Dependencies.Select(Visit).OfType<Package>().ToList();
                    var matches = criteria.MatchesPackage(package.Name, package.Version);
                    var result = matches || children.Count > 0
                        ? new Package(package.Name, package.Version, matches && criteria.HasPackageFilter,
                            children.Count > 0 ? children : null)
                        : null;

                    visiting.Remove(name);
                    cache.Add(name, result);
                    return result;
                }
            }

            if (targets.Count > 0) projects.Add(new Project(project.Name, project.Version, targets));
        }

        return projects.Count > 0 ? new Solution(graph.Name, projects) : null;
    }
}
