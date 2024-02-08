using Splat;

namespace DotNetWhere.Application.Loggers;

internal class OutputWriterFactory(
    Options options
    )
{
    public IOutputWriter CreateLogger() =>
        Locator.Current.GetService<IOutputWriter>(options.OutputFormat.ToString())!;
}
