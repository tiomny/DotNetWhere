namespace DotNetWhere.Application.Commands;

[Description("Shows information about why a NuGet package is installed")]
internal sealed partial class DotNetWhereCommand
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<DIR>")]
        [Description("Solution directory")]
        public string Directory { get; init; }

        [CommandArgument(1, "<NAME>")]
        [Description("The NuGet package name")]
        public string PackageName { get; init; }

        [CommandOption("-v|--version <VERSION>")]
        [Description("The NuGet package version")]
        public string PackageVersion { get; init; }

        [CommandOption("-f|--format")]
        [Description("Output format")]
        public OutputFormat OutputFormat { get; init; }

        [CommandOption("-o|--output")]
        [Description("Output file")]
        public string OutputFile { get; init; }
    }

    public enum OutputFormat
    {
        Compact,
        Color,
        Yaml,
        Json
    }
}