namespace DotNetWhere.Core.Models;

public sealed class Response
{
    internal Response(IEnumerable<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    internal Response(Solution? solution)
    {
        IsSuccess = true;
        Solution = solution;
    }

    public bool IsSuccess { get; }
    public IEnumerable<string> Errors { get; } = Enumerable.Empty<string>();
    public Solution? Solution { get; }
}