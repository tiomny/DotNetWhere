using DotNetWhere.Application.Writers;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere.Application.Factories;

internal class WriterFactory(
    IServiceProvider provider,
    Options options
    ) : IFactory<IWriter>
{
    public IWriter Create() =>
        provider.GetKeyedService<IWriter>(options.OutputFormat)!;
}
