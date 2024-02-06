namespace DotNetWhere.Core.Matchers;

internal static class MatcherFactory
{
    public static IMatcher CreateMatcher(string name)
    {
        if (name.Contains('*') || name.Contains('?'))
        {
            return new MaskMatcher(name);
        }
        else
        {
            return new ExactMatcher(name);
        }
    }
}
