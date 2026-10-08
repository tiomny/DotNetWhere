namespace DotNetWhere.Core.Models;

public sealed class Solution(
    string name,
    IEnumerable<Project> projects
    )
{
    public string Name { get; } = name;
    public IReadOnlyList<Project> Projects { get; } = Array.AsReadOnly(projects.ToArray());
}
