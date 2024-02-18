namespace DotNetWhere.Core.Matchers;

internal class NegateMatcher<T>
    (IMatcher<T> matcher) : IMatcher<T>
{
    public bool Match(T test) => !matcher.Match(test);
}
