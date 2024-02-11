using Microsoft.Extensions.DependencyInjection;

namespace DotNetWhere.Core;

public static class Extensions
{
    public static IServiceCollection AddCore(this IServiceCollection services) =>
        services.AddScoped<IProvider, Provider>();
}