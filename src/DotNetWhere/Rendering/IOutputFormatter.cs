using DotNetWhere.Application;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Rendering;

internal interface IOutputFormatter
{
    void WriteSolution(TextWriter output, Solution solution, TimeSpan elapsedTime);
    void WriteErrors(TextWriter output, IEnumerable<AnalysisError> errors);
}
