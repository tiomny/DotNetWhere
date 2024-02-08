using CommandLine;
using CommandLine.Text;
using Splat;

namespace DotNetWhere.Application;

public static class Cli
{
    public static void Run(IEnumerable<string> args)
    {
        var parser = new CommandLine.Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments<Options>(args);
        parserResult
          .WithParsed<Options>(options => Run(options))
          .WithNotParsed(_ => DisplayHelp(parserResult));
    }

    static void Run(Options options)
    {
        RegisterServices(options);

        Locator.Current.GetService<DotNetWhereCommand>()!.Execute();
    }

    static void RegisterServices(Options options)
    {
        SplatRegistrations.SetupIOC();

        Locator.CurrentMutable.AddCore();

        SplatRegistrations.RegisterConstant(options);
        SplatRegistrations.RegisterLazySingleton<OutputWriterFactory>();
        Locator.CurrentMutable.RegisterLazySingleton<IOutputWriter>(
            () => Locator.Current.GetService<OutputWriterFactory>()!.CreateLogger()
            );
#pragma warning disable SPLATDI006 // Interface has been registered before
        SplatRegistrations.RegisterLazySingleton<IOutputWriter, CompactOutputWriter>(CompactOutputWriter.Contract);
        SplatRegistrations.RegisterLazySingleton<IOutputWriter, ColorOutputWriter>(ColorOutputWriter.Contract);
        SplatRegistrations.Register<IOutputWriter, JsonOutputWriter>(JsonOutputWriter.Contract);
        SplatRegistrations.Register<IOutputWriter, YamlOutputWriter>(YamlOutputWriter.Contract);
#pragma warning restore SPLATDI006 // Interface has been registered before
        SplatRegistrations.RegisterLazySingleton<DotNetWhereCommand>();
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
}