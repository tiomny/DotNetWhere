namespace DotNetWhere.Core.Graph;

public sealed class DependencyGraph(string name, IEnumerable<ProjectGraph> projects)
{
    public string Name { get; } = name;
    public IReadOnlyList<ProjectGraph> Projects { get; } = Array.AsReadOnly(projects.ToArray());
}
