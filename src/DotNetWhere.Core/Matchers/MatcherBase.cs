
namespace DotNetWhere.Core.Matchers;

internal abstract class MatcherBase(
    string search
    ) : IMatcher
{
    protected string Search { get; } = search;
    public abstract bool Match(string name);
}
