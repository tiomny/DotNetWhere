using CommandLine;

namespace DotNetWhere.Application;

internal sealed class Options
{
    [Option(shortName: 'd', longName: "dir", HelpText = "Solution directory. When not passed: current directory.")]
    public string? Directory { get; init; }

    [Option(shortName: 'p', longName: "package", HelpText = "The NuGet package name. When not passed: all packages. Can be mask or regex.")]
    public string? PackageName { get; init; }

    [Option(shortName: 'V', longName: "package-version", HelpText = "The NuGet package version. When not passed: any version. Can be mask or regex.")]
    public string? PackageVersion { get; init; }

    [Option(shortName: 'f', longName: "format", HelpText = "Output format. Default: Compact.")]
    public OutputFormat OutputFormat { get; init; }

    [Option(shortName: 'o', longName: "output", HelpText = "Output file.")]
    public string? OutputFile { get; init; }
}
