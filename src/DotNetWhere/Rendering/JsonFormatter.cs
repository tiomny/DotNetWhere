using DotNetWhere.Application;
using DotNetWhere.Core.Models;

using Newtonsoft.Json;

namespace DotNetWhere.Rendering;

internal sealed class JsonFormatter : IOutputFormatter
{
    public void WriteSolution(TextWriter output, Solution solution, TimeSpan elapsedTime) =>
        WriteObject(output, new { solution = SolutionOutput.From(solution), elapsedTime });

    public void WriteErrors(TextWriter output, IEnumerable<AnalysisError> errors) =>
        WriteObject(output, new { errors = errors.Select(error => error.Message).ToArray() });

    private static void WriteObject(TextWriter output, object value) =>
        output.WriteLine(JsonConvert.SerializeObject(value,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
}
