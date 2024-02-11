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
          .WithParsed<Options>(options => Run(options))
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
            .RegisterFactory<TextWriter, OutputWriterFactory>()
            .RegisterFactory<IWriter, WriterFactory>()
            .AddKeyedTransient<IWriter, CompactWriter>(OutputFormat.Compact)
            .AddKeyedTransient<IWriter, ColorWriter>(OutputFormat.Color)
            .AddKeyedTransient<IWriter, JsonWriter>(OutputFormat.Json)
            .AddKeyedTransient<IWriter, YamlWriter>(OutputFormat.Yaml)
            .AddSingleton<DotNetWhereCommand>();
    }

    static void DisplayHelp<T>(ParserResult<T> result)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AddEnumValuesToHelpText = true;
            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        Console.WriteLine(helpText);
    }

    static IServiceCollection RegisterFactory<TService, TFactory>(this IServiceCollection services)
        where TService : class
        where TFactory : class, IFactory<TService>
    {
        services.AddSingleton<TFactory>();
        services.AddSingleton<TService>(
            provider => provider.GetService<TFactory>()!.Create()
            );

        return services;
    }
}