namespace DotNetWhere.Core.Models;

public sealed class Target(
    string version,
    IEnumerable<Package> packages
    )
{
    public string Version { get; } = version;
    public IReadOnlyList<Package> Packages { get; } = Array.AsReadOnly(packages.ToArray());
}
