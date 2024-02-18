using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Matchers;

internal class TargetMatcher
    (Func<TargetFrameworkInformation, bool> matcherFunc)
    : DynamicMatcher<TargetFrameworkInformation>(matcherFunc), ITargetMatcher;