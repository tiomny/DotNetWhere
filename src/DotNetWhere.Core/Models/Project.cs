namespace DotNetWhere.Core.Models;

public class Project(
    string name,
    string? version
    )
{
    public string Name { get; } = name;
    public string? Version { get; } = version;
    public List<Target> Targets { get; } = [];
}
