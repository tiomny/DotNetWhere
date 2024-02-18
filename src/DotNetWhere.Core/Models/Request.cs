namespace DotNetWhere.Core.Models;

public sealed class Request
    (string workingDirectory)
{
    public string? PackageName { get; init; }
    public string? PackageVersion { get; init; }
    public string? Target {  get; init; }
    public string WorkingDirectory { get; } = workingDirectory;
}