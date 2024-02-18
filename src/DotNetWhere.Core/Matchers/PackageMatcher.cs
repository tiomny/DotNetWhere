using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Matchers;

internal class PackageMatcher
    (Func<PackageKey, bool> matcherFunc)
    : DynamicMatcher<PackageKey>(matcherFunc), IPackageMatcher;
