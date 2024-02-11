using System.Text.RegularExpressions;

namespace DotNetWhere.Core.Models;

public class Package(
    string name,
    string version,
    bool isMatch,
    List<Package>? packages
    )
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public bool IsMatch { get; } = isMatch;
    public List<Package>? Packages { get; } = packages;
    public override string ToString() => $"{Name}: {Version}";
}
