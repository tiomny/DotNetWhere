using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Loggers;

internal class YamlLogger : ILogger
{
    public void Log(Response response) => throw new NotImplementedException();
    public void Log(ElapsedTime elapsedTime) => throw new NotImplementedException();
    public Response LogAction(Func<Response> getResponse) => throw new NotImplementedException();
    public void LogErrors(IEnumerable<string> errors) => throw new NotImplementedException();
}
