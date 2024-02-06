using System.IO.Enumeration;

namespace DotNetWhere.Core.Matchers;

internal class MaskMatcher(string search) : MatcherBase(search)
{
    public override bool Match(string name) =>
        FileSystemName.MatchesSimpleExpression(Search, name);
}
