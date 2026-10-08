namespace DotNetWhere.Core.Graph;

public sealed class ResolvedPackage(string name, string version, IEnumerable<string> dependencies)
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public IReadOnlyList<string> Dependencies { get; } = Array.AsReadOnly(dependencies.ToArray());
}
