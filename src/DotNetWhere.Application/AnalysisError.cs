namespace DotNetWhere.Application;

public enum AnalysisErrorCode
{
    InvalidRequest,
    InvalidFilter,
    InvalidDirectory,
    ProcessFailed,
    InvalidAssets,
    InvalidGraph,
    PackageNotFound
}

public sealed record AnalysisError(AnalysisErrorCode Code, string Message);
