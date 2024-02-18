using DotNetWhere.Core.Factories;
using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Resolvers;

#nullable disable
internal class TargetMatcherResolver(
    Request request
    ) : IResolver<ITargetMatcher>
{
    public ITargetMatcher Get()
    {
        var target = request.Target;
        if (string.IsNullOrEmpty(target))
        {
            return null;
        }

        var matcher = StringMatcherFactory.CreateMatcher(target)!;

        return new TargetMatcher(targetFramework => matcher.Match(targetFramework.ToString()));
    }
}
#nullable restore