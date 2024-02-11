using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Writers;

internal abstract class WriterBase : IWriter
{
    protected readonly TextWriter _writer;

    protected WriterBase(TextWriter writer) => _writer = writer;

    public abstract void WriteSolution(Solution solution, ElapsedTime elapsedTime);
    public abstract void WriteErrors(IEnumerable<string> errors);
}
