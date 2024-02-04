namespace DotNetWhere.Application.Commands;

internal sealed partial class DotNetWhereCommand
(
    ILogger logger,
    IProvider provider
) : Command<DotNetWhereCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = logger.LogAction(settings.ToRequest);
        var response = logger.LogAction(() => provider.Get(request));
        logger.Log(request, response);
        logger.Log(stopwatch.Elapsed);

        return default;
    }
}