using System.Text.RegularExpressions;

using DotNetWhere.Core.Search;

namespace DotNetWhere.Application;

public sealed class AnalyzeDependencies(IDependencyGraphSource source)
{
    public async Task<AnalysisResult> ExecuteAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(request.Directory))
            return AnalysisResult.Failure(AnalysisErrorCode.InvalidRequest, "An input directory is required.");

        SearchCriteria criteria;
        try
        {
            criteria = new SearchCriteria(request.PackageName, request.PackageVersion, request.Target);
        }
        catch (ArgumentException exception)
        {
            return AnalysisResult.Failure(AnalysisErrorCode.InvalidFilter, exception.Message);
        }

        try
        {
            var graph = await source.LoadAsync(request.Directory, cancellationToken);
            var solution = DependencySearch.Find(graph, criteria, cancellationToken);
            return solution is not null
                ? AnalysisResult.Success(solution)
                : AnalysisResult.Failure(AnalysisErrorCode.PackageNotFound, "Package not found.");
        }
        catch (GraphSourceException exception)
        {
            return AnalysisResult.Failure(exception.Code, exception.Message);
        }
        catch (InvalidGraphException exception)
        {
            return AnalysisResult.Failure(AnalysisErrorCode.InvalidGraph, exception.Message);
        }
        catch (RegexMatchTimeoutException exception)
        {
            return AnalysisResult.Failure(AnalysisErrorCode.InvalidFilter, exception.Message);
        }
    }
}
