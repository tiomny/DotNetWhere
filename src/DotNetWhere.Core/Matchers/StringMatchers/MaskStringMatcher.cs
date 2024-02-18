using System.IO.Enumeration;

namespace DotNetWhere.Core.Matchers.StringMatchers;

internal class MaskStringMatcher
    (string search) : StringMatcherBase(search)
{
    public override bool Match(string test) =>
        FileSystemName.MatchesSimpleExpression(Search, test);
}
