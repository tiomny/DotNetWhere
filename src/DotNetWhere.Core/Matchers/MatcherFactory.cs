namespace DotNetWhere.Core.Matchers;

internal static class MatcherFactory
{
    public static IMatcher CreateMatcher(string name)
    {
        if (name.StartsWith('/'))
            return new RegexMatcher(name);
        
        if (name.Contains('*') || name.Contains('?'))
            return new MaskMatcher(name);
        
        return new ExactMatcher(name);
    }
}
