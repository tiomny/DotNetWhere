namespace DotNetWhere.Core.Matchers;

internal class ExactMatcher(string search) : MatcherBase(search)
{
    public override bool Match(string name) =>
        name.Equals(Search, StringComparison.InvariantCultureIgnoreCase);
}
