using System.Text.RegularExpressions;

namespace DotNetWhere.Core.Matchers;

internal class RegexMatcher(string search) : MatcherBase(search)
{
    public override bool Match(string name) =>
        Regex.IsMatch(name, Search);
}
