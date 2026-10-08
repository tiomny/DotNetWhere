using System.Collections.Frozen;

namespace DotNetWhere.Core.Graph;

public sealed class TargetGraph(
    string framework,
    IEnumerable<string> roots,
    IEnumerable<ResolvedPackage> packages)
{
    public string Framework { get; } = framework;
    public IReadOnlyList<string> Roots { get; } = Array.AsReadOnly(roots.ToArray());
    public IReadOnlyDictionary<string, ResolvedPackage> Packages { get; } =
        packages.ToFrozenDictionary(package => package.Name, StringComparer.OrdinalIgnoreCase);
}
