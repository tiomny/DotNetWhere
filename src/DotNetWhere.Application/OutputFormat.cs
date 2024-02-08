using CommandLine;

namespace DotNetWhere.Application;

internal sealed class Options
{
    [Option(shortName: 'd', longName: "dir", HelpText = "Solution directory")]
    public string? Directory { get; init; }

    [Option(shortName: 'p', longName: "package", HelpText = "The NuGet package name")]
    public string? PackageName { get; init; }

    [Option(shortName: 'V', longName: "package-version", HelpText = "The NuGet package version")]
    public string? PackageVersion { get; init; }

    [Option(shortName: 'f', longName: "format", HelpText = "Output format")]
    public OutputFormat OutputFormat { get; init; }

    [Option(shortName: 'o', longName: "output", HelpText = "Output file")]
    public string? OutputFile { get; init; }
}

public enum OutputFormat
{
    Compact,
    Color,
    Yaml,
    Json
}
