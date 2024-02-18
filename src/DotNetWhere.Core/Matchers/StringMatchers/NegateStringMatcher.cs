using DotNetWhere.Core.Factories;

namespace DotNetWhere.Core.Matchers.StringMatchers;

internal class NegateStringMatcher
    (string search) : NegateMatcher<string>(StringMatcherFactory.CreateMatcher(search)!), IStringMatcher;
