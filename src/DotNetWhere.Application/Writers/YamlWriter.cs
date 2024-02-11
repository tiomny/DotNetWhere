using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Writers;

internal class YamlWriter(TextWriter writer) : WriterBase(writer)
{
    public override void WriteErrors(IEnumerable<string> errors) => throw new NotImplementedException();
    public override void WriteSolution(Solution solution, ElapsedTime elapsedTime) => throw new NotImplementedException();
}
