using DotNetWhere.Application.Writers;
using DotNetWhere.Core.Resolvers;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere.Application.Factories;

internal class WriterResolver(
    IServiceProvider provider,
    Options options
    ) : IResolver<IWriter>
{
    public IWriter Get() =>
        provider.GetRequiredKeyedService<IWriter>(options.OutputFormat);
}
