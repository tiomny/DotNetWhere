namespace DotNetWhere.Core.Models;

public sealed class Package(
    string name,
    string version,
    bool isMatch,
    IEnumerable<Package>? packages
    )
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public bool IsMatch { get; } = isMatch;
    public IReadOnlyList<Package>? Packages { get; } = packages is null
        ? null
        : Array.AsReadOnly(packages.ToArray());
    public override string ToString() => $"{Name}: {Version}";
}
