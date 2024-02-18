namespace DotNetWhere.Core.Matchers;

internal class DynamicMatcher<T>
    (Func<T, bool> matcherFunc) : IMatcher<T>
{
    public bool Match(T test) => matcherFunc(test);
}
