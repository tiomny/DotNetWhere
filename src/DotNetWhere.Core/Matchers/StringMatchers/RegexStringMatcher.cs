using System.Text.RegularExpressions;

namespace DotNetWhere.Core.Matchers.StringMatchers;

internal class RegexStringMatcher(string search) : StringMatcherBase(search)
{
    public override bool Match(string test) =>
        Regex.IsMatch(test, Search);
}
