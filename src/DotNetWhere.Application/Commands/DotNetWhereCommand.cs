using DotNetWhere.Application.Writers;
using DotNetWhere.Core.Providers;

namespace DotNetWhere.Application.Commands;

internal sealed class DotNetWhereCommand
(
    IWriter outputWriter,
    IProvider provider,
    Options options
)
{
    public int Execute()
    {
        var stopwatch = Stopwatch.StartNew();
        var request = options.ToRequest();
        var response = provider.Get(request);

        stopwatch.Stop();

        if (response.IsSuccess)
        {
            outputWriter.WriteSolution(response.Solution!, stopwatch.Elapsed);
        }
        else
        {
            outputWriter.WriteErrors(response.Errors);
            return -1;
        }

        return default;
    }
}