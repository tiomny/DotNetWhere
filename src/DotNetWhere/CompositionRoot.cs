using DotNetWhere.Application;
using DotNetWhere.Infrastructure;
using DotNetWhere.Rendering;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere;

internal static class CompositionRoot
{
    public static ServiceProvider BuildServiceProvider() => new ServiceCollection()
        .AddSingleton(new DotNetProcessOptions())
        .AddTransient<DotNetProcessRunner>()
        .AddTransient<AssetsGraphReader>()
        .AddTransient<IDependencyGraphSource, NuGetDependencyGraphSource>()
        .AddTransient<AnalyzeDependencies>()
        .AddTransient<OutputFormatterFactory>()
        .AddTransient<Cli>()
        .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
