using System.Text.RegularExpressions;

namespace DotNetWhere.Core.Models;

public class Package(
    string name,
    string version,
    bool isMatch
    )
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public bool IsMatch { get; } = isMatch;
    public List<Package> Packages { get; } = [];
    public override string ToString() => $"{Name}: {Version}";
}
