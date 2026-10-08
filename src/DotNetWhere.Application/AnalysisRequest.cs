namespace DotNetWhere.Application;

public sealed record AnalysisRequest(
    string Directory,
    string? PackageName = null,
    string? PackageVersion = null,
    string? Target = null);
