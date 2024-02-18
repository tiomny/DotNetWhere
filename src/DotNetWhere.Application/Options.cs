using CommandLine;
using CommandLine.Text;

namespace DotNetWhere.Application;

internal sealed class Options
{
    [Option(shortName: 'd', longName: "dir", HelpText = "Solution directory. When not passed: current directory.")]
    public string? Directory { get; init; }

    [Option(shortName: 'p', longName: "package", HelpText = "The NuGet package name. ***")]
    public string? PackageName { get; init; }

    [Option(shortName: 'V', longName: "package-version", HelpText = "The NuGet package version. ***")]
    public string? PackageVersion { get; init; }

    [Option(shortName: 't', longName: "dotnet-target", HelpText = ".Net target version. ***")]
    public string? Target { get; init; }

    [Option(shortName: 'f', longName: "format", HelpText = "Output format. Default: Color.")]
    public OutputFormat OutputFormat { get; init; }

    [Option(shortName: 'o', longName: "output", HelpText = "Output file.")]
    public string? OutputFile { get; init; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("List all packages in solution in current directory with compact format", new Options() { OutputFormat = OutputFormat.Compact });
            yield return new Example("List packages in target net8.0 that name matches System.* and version is not 6.0.0 ", new UnParserSettings() { PreferShortName = true }, new Options { PackageName = "System.*", PackageVersion = @"!6.0.0", Target = "net8.0" });
        }
    }
}
