namespace DotNetWhere.Core.Models;

public class Target(
    string version,
    List<Package> packages
    )
{
    public string Version { get; } = version;
    public List<Package> Packages { get; } = packages;
}
