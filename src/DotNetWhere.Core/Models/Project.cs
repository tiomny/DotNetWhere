namespace DotNetWhere.Core.Models;

public sealed class Project(
    string name,
    string? version,
    IEnumerable<Target> targets
    )
{
    public string Name { get; } = name;
    public string? Version { get; } = version;
    public IReadOnlyList<Target> Targets { get; } = Array.AsReadOnly(targets.ToArray());
    public override string ToString() => $"{Name}: {Version}";
}
