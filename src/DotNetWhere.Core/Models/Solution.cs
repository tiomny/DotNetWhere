namespace DotNetWhere.Core.Models;

public class Solution(
    string name)
{
    public string Name { get; } = name;
    public List<Project> Projects { get; } = [];
}
