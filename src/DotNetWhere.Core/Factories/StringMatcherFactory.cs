using DotNetWhere.Core.Matchers.StringMatchers;

namespace DotNetWhere.Core.Factories;

internal static class StringMatcherFactory
{
    public static IStringMatcher? CreateMatcher(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (name.StartsWith('!'))
            return new NegateStringMatcher(name[1..]);


        if (name.StartsWith('/'))
            return new RegexStringMatcher(name[1..]);

        if (name.Contains('*') || name.Contains('?'))
            return new MaskStringMatcher(name);

        return new ExactStringMatcher(name);
    }
}
