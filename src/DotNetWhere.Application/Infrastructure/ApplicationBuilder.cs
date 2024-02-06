using OutputFormat = DotNetWhere.Application.Commands.DotNetWhereCommand.OutputFormat;

namespace DotNetWhere.Application.Infrastructure;

internal static class ApplicationBuilder
{
    private static readonly IServiceCollection Services = new ServiceCollection();

    public static IServiceCollection AddServices() =>
        Services
            .AddCore()
            .AddSingleton<LoggerFactory>()
            .AddKeyedSingleton<ILogger, CompactLogger>(OutputFormat.Compact)
            .AddKeyedSingleton<ILogger, ColorLogger>(OutputFormat.Color)
            .AddKeyedSingleton<ILogger, JsonLogger>(OutputFormat.Json)
            .AddKeyedSingleton<ILogger, YamlLogger>(OutputFormat.Yaml)
        ;

    public static ITypeRegistrar AsTypeRegistrar(this IServiceCollection services) =>
        new TypeRegistrar(services);

    public static ICommandApp ForCommandApplication<TCommand>(
        this ITypeRegistrar typeRegistrar,
        string applicationName = null)
        where TCommand : class, ICommand
    {
        var commandApplication = new CommandApp<TCommand>(typeRegistrar);

        if (!string.IsNullOrEmpty(applicationName))
            commandApplication.Configure(configurator => configurator.SetApplicationName(applicationName));

        return commandApplication;
    }
}