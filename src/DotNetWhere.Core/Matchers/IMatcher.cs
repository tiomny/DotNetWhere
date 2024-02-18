namespace DotNetWhere.Core.Matchers;

internal interface IMatcher<in T>
{
    bool Match(T test);
}
