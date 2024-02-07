using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Loggers;

internal interface ILogger
{
    Response LogAction(Func<Response> getResponse);
    void Log(Response response);
    void Log(ElapsedTime elapsedTime);
    void LogErrors(IEnumerable<string> errors);
}