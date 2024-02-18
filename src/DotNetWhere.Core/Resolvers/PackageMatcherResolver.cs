using System.Linq.Expressions;

using DotNetWhere.Core.Factories;
using DotNetWhere.Core.Helpers;
using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Resolvers;

#nullable disable
internal class PackageMatcherResolver(
    Request request
    ) : IResolver<PackageMatcher>

{
    public PackageMatcher Get()
    {
        var packageName = request.PackageName;
        var packageVersion = request.PackageVersion;

        var noName = string.IsNullOrEmpty(packageName);
        var noVersion = string.IsNullOrEmpty(packageVersion);

        if (noName && noVersion)
        {
            return null;
        }

        var nameMatcher = StringMatcherFactory.CreateMatcher(packageName);
        var versionMatcher = StringMatcherFactory.CreateMatcher(packageVersion);

        Expression<Func<PackageKey, bool>> nameExpr =
            packageKey => nameMatcher.Match(packageKey.Name);

        Expression<Func<PackageKey, bool>> versionExpr =
            packageKey => versionMatcher.Match(packageKey.Version);

        var result = Expression.Lambda<Func<PackageKey, bool>>(
            Expression.And(
                nameMatcher == null
                    ? Expression.Constant(true)
                    : nameExpr.Body,
                versionMatcher == null
                    ? Expression.Constant(true)
                    : new ExpressionParameterReplacer(versionExpr.Parameters, nameExpr.Parameters).Visit(versionExpr.Body)
            ),
            nameExpr.Parameters);

        var matcherFunc = result.Compile();

        return new PackageMatcher(matcherFunc);
    }
}
#nullable restore