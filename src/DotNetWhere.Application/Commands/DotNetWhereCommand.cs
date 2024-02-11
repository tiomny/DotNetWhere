using DotNetWhere.Core.Providers;

namespace DotNetWhere.Application.Commands;

internal sealed class DotNetWhereCommand
(
    IOutputWriter outputWriter,
    IProvider provider,
    Options options
)
{
    public int Execute()
    {
        var stopwatch = Stopwatch.StartNew();
        var request = options.ToRequest();
        var response = outputWriter.LogAction(() => provider.Get(request));

        stopwatch.Stop();

        if (response.IsSuccess)
        {
            outputWriter.Log(response);
        }
        else
        {
            outputWriter.LogErrors(response.Errors);
            return -1;
        }
        outputWriter.Log(stopwatch.Elapsed);

        return default;
    }
}