using Splat;

namespace DotNetWhere.Core;

public static class Extensions
{
    public static IMutableDependencyResolver AddCore(this IMutableDependencyResolver resolver) =>
        resolver.RegisterAnd<IProvider, Provider>();
}