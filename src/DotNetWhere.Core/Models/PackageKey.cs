namespace DotNetWhere.Core.Models;

internal class PackageKey(
    string name,
    string version,
    string target) : IEquatable<PackageKey>
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Target { get; } = target;

    public override bool Equals(object? obj) =>
        obj is PackageKey other && Equals(other);

    public bool Equals(PackageKey? other) =>
        other != null &&
        Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase) &&
        Version.Equals(other.Version, StringComparison.InvariantCultureIgnoreCase) &&
        Target.Equals(other.Target, StringComparison.InvariantCultureIgnoreCase)
        ;

    public override int GetHashCode() => Name.GetHashCode() ^ Version.GetHashCode() ^ Target.GetHashCode();
}
