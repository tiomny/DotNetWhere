using CommandLine;
using CommandLine.Text;

using DotNetWhere.Application.Factories;
using DotNetWhere.Application.Writers;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere.Application;

public static class Cli
{
    public static void Run(IEnumerable<string> args)
    {
        using var parser = new CommandLine.Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<Options>(args);
        parserResult
          .WithParsed(Run)
          .WithNotParsed(_ => DisplayHelp(parserResult));
    }

    static void Run(Options options)
    {
        var services = new ServiceCollection();

        RegisterServices(services, options);

        using var provider = services.BuildServiceProvider();

        provider.GetService<DotNetWhereCommand>()!.Execute();
    }

    static void RegisterServices(IServiceCollection services, Options options)
    {
        services
            .AddCore()
            .AddSingleton(options)
            .AddSingleton(options.ToRequest())
            .RegisterResolver<TextWriter, OutputWriterResolver>()
            .RegisterResolver<IWriter, WriterResolver>()
            .AddKeyedSingleton<IWriter, CompactWriter>(OutputFormat.Compact)
            .AddKeyedSingleton<IWriter, ColorWriter>(OutputFormat.Color)
            .AddKeyedSingleton<IWriter, JsonWriter>(OutputFormat.Json)
            .AddKeyedSingleton<IWriter, YamlWriter>(OutputFormat.Yaml)
            .AddSingleton<DotNetWhereCommand>();
    }

    static void DisplayHelp<T>(ParserResult<T> result)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AddEnumValuesToHelpText = true;
            h.AddPostOptionsLine("*     When not passed: any.");
            h.AddPostOptionsLine("**    Can be mask or regex.");
            h.AddPostOptionsLine("***   search expression can start from `!` or `/`.");
            h.AddPostOptionsLine("        `!` is for `not`.");
            h.AddPostOptionsLine("            Everything afer it will be considered as a `not match` expression.");
            h.AddPostOptionsLine("            The app looks for everything that does not match expression after `!`.");
            h.AddPostOptionsLine("        `/` is for `regular expression`.");
            h.AddPostOptionsLine("            The app looks for everything that match regular expression after `/`.");
            h.AddPostOptionsLine(string.Empty);

            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        Console.WriteLine(helpText);
    }

}