using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Writers;

internal class CompactWriter(TextWriter writer) : WriterBase(writer)
{
    public override void WriteSolution(Solution solution, ElapsedTime elapsedTime)
    {
        foreach (var project in solution.Projects)
        {
            WriteLine(message: project.Name);
            foreach (var target in project.Targets)
            {
                WriteLine(2, target.Version);
                foreach (var package in target.Packages)
                {
                    WritePackage(4, package!);
                }
            }
        }
        WriteLine(message: $@"Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}");
    }
    public override void WriteErrors(IEnumerable<string> errors)
    {
        WriteLine(message: "Errors:");
        foreach (var error in errors)
            WriteLine(2, error);
    }

    private void WritePackage(int pad, Package package)
    {
        WriteLine(pad, package.ToString());
        if (package.Packages is not null)
        {
            foreach (var dep in package.Packages)
            {
                WritePackage(pad + 2, dep);
            }
        }
    }

    private void WriteLine(int pad = 0, string? message = null) =>
        _writer.WriteLine("".PadLeft(pad) + message);
}
