namespace DotNetWhere.Core.Models;

public class Project(
    string name,
    string? version,
    List<Target> targets
    )
{
    public string Name { get; } = name;
    public string? Version { get; } = version;
    public List<Target> Targets { get; } = targets;
}
