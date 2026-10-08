using DotNetWhere.Core.Models;

namespace DotNetWhere.Application;

public sealed class AnalysisResult
{
    private AnalysisResult(Solution? solution, IReadOnlyList<AnalysisError> errors)
    {
        Solution = solution;
        Errors = errors;
    }

    public bool IsSuccess => Solution is not null;
    public Solution? Solution { get; }
    public IReadOnlyList<AnalysisError> Errors { get; }

    public static AnalysisResult Success(Solution solution) => new(solution, Array.Empty<AnalysisError>());
    public static AnalysisResult Failure(AnalysisErrorCode code, string message) =>
        new(null, Array.AsReadOnly(new[] { new AnalysisError(code, message) }));
}
