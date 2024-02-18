using DotNetWhere.Application.Writers;
using DotNetWhere.Core.Models;
using DotNetWhere.Core.Providers;

namespace DotNetWhere.Application.Commands;

internal sealed class DotNetWhereCommand
(
    IWriter outputWriter,
    IProvider provider,
    Request request
)
{
    public int Execute()
    {
        var stopwatch = Stopwatch.StartNew();
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