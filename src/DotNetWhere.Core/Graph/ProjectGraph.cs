namespace DotNetWhere.Core.Graph;

public sealed class ProjectGraph(string name, string? version, IEnumerable<TargetGraph> targets)
{
    public string Name { get; } = name;
    public string? Version { get; } = version;
    public IReadOnlyList<TargetGraph> Targets { get; } = Array.AsReadOnly(targets.ToArray());
}
