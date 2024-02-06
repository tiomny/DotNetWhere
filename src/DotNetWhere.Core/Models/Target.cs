namespace DotNetWhere.Core.Models;

public class Target(
    string version
    )
{
    public string Version { get; } = version;
    public List<Package?> Packages { get; } = [];
}
