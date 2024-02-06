using OutputFormat = DotNetWhere.Application.Commands.DotNetWhereCommand.OutputFormat;

namespace DotNetWhere.Application.Loggers;

internal class LoggerFactory(IServiceProvider serviceProvider)
{
    public ILogger CreateLogger(OutputFormat outputFormat) =>
        serviceProvider.GetKeyedService<ILogger>(outputFormat);
}
