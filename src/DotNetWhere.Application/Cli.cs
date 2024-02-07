namespace DotNetWhere.Application;

public static class Cli
{
    public static void Run(IEnumerable<string> args) =>
        ApplicationBuilder
            .AddServices()
            .AsTypeRegistrar()
            .ForCommandApplication<DotNetWhereCommand>("dotnet where")
            .Run(args);
}