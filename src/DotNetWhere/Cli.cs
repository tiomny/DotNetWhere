using System.Diagnostics;
using System.Reflection;

using CommandLine;
using CommandLine.Text;

using DotNetWhere.Application;
using DotNetWhere.Rendering;

namespace DotNetWhere;

internal sealed class Cli(AnalyzeDependencies analysis, OutputFormatterFactory formatters)
{
    public async Task<int> RunAsync(
        IEnumerable<string> args,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        using var parser = new Parser(settings => settings.HelpWriter = null);
        var parsed = parser.ParseArguments<Options>(args);
        if (parsed is NotParsed<Options> notParsed)
        {
            if (notParsed.Errors.Any(parseError => parseError is VersionRequestedError))
            {
                output.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                return 0;
            }

            var helpRequested = notParsed.Errors.All(parseError => parseError is HelpRequestedError);
            (helpRequested ? output : error).WriteLine(CreateHelp(parsed));
            return helpRequested ? 0 : 2;
        }

        var options = ((Parsed<Options>)parsed).Value;
        if (options.OutputFormat == OutputFormat.Yaml)
        {
            error.WriteLine("Yaml output is not implemented. Choose Color, Compact or Json.");
            return 2;
        }

        try
        {
            var request = new AnalysisRequest(options.Directory ?? Environment.CurrentDirectory,
                options.PackageName, options.PackageVersion, options.Target);
            var stopwatch = Stopwatch.StartNew();
            var result = await analysis.ExecuteAsync(request, cancellationToken);
            stopwatch.Stop();
            var formatter = formatters.Create(options.OutputFormat);

            if (!result.IsSuccess)
            {
                // Failed queries must not truncate an existing destination file.
                formatter.WriteErrors(output, result.Errors);
                return 1;
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                formatter.WriteSolution(output, result.Solution!, stopwatch.Elapsed);
            }
            else
            {
                using var destination = new StreamWriter(options.OutputFile);
                formatter.WriteSolution(destination, result.Solution!, stopwatch.Elapsed);
            }

            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            error.WriteLine("Operation cancelled.");
            return 130;
        }
        catch (Exception exception)
        {
            error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
    }

    private static HelpText CreateHelp(ParserResult<Options> result) => HelpText.AutoBuild(result, help =>
    {
        help.AddEnumValuesToHelpText = true;
        help.AddPostOptionsLine("All query arguments are optional. An omitted filter matches any value.");
        help.AddPostOptionsLine("Use * or ? for wildcards, / for regex, and ! to negate an expression.");
        help.AddPostOptionsLine("Yaml output is not implemented.");
        return HelpText.DefaultParsingErrorsHandler(result, help);
    }, example => example);
}
