namespace DotNetWhere.Application;

public sealed class GraphSourceException(AnalysisErrorCode code, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public AnalysisErrorCode Code { get; } = code;
}
