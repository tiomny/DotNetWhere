using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Resolvers;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere.Core;

public static class Extensions
{
    public static IServiceCollection AddCore(this IServiceCollection services) =>
        services
            .RegisterResolver<ITargetMatcher, TargetMatcherResolver>()
            .RegisterResolver<IPackageMatcher, PackageMatcherResolver>()
            .AddScoped<IProvider, Provider>();

    public static IServiceCollection RegisterResolver<TService, TResolver>(this IServiceCollection services)
        where TService : class
        where TResolver : class, IResolver<TService> =>
            services
                .AddSingleton<IResolver<TService>, TResolver>()
                .AddSingleton<TService>(
                    provider => provider.GetRequiredService<IResolver<TService>>().Get()
                );
}