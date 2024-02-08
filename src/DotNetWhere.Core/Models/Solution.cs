namespace DotNetWhere.Core.Models;

public class Solution(
    string name,
    List<Project> projects
    )
{
    public string Name { get; } = name;
    public List<Project> Projects { get; } = projects;
}
