namespace DotNetWhere.Core.Matchers;

internal static class MatcherFactory
{
    public static IMatcher? CreateMatcher(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (name.StartsWith('/'))
            return new RegexMatcher(name[1..]);
        
        if (name.Contains('*') || name.Contains('?'))
            return new MaskMatcher(name);
        
        return new ExactMatcher(name);
    }
}
