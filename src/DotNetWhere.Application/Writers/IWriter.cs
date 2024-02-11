using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Writers;

internal interface IWriter
{
    void WriteSolution(Solution solution, ElapsedTime elapsedTime);
    void WriteErrors(IEnumerable<string> errors);
}
