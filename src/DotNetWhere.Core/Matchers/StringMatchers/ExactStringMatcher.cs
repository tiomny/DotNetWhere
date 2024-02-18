namespace DotNetWhere.Core.Matchers.StringMatchers;

internal class ExactStringMatcher
    (string search) : StringMatcherBase(search)
{
    public override bool Match(string test) =>
        test.Equals(Search, StringComparison.InvariantCultureIgnoreCase);
}
