namespace DotNetWhere.Core.Matchers.StringMatchers;

internal abstract class StringMatcherBase
    (
    string search
    ) : IStringMatcher
{
    protected string Search { get; } = search;
    public abstract bool Match(string test);
}
