namespace DotNetWhere.Application.Commands;

internal sealed partial class DotNetWhereCommand
(
    LoggerFactory loggerFactory,
    IProvider provider
) : Command<DotNetWhereCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        var logger = loggerFactory.CreateLogger(settings.OutputFormat);

        var stopwatch = Stopwatch.StartNew();
        var request = settings.ToRequest();
        var response = logger.LogAction(() => provider.Get(request));
        if (response.IsSuccess)
        {
            logger.Log(response);
        }
        else
        {
            logger.LogErrors(response.Errors);
        }
        logger.Log(stopwatch.Elapsed);

        return default;
    }
}