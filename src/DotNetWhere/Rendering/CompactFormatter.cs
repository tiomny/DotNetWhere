using DotNetWhere.Application;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Rendering;

internal sealed class CompactFormatter : IOutputFormatter
{
    public void WriteSolution(TextWriter output, Solution solution, TimeSpan elapsedTime)
    {
        foreach (var project in SolutionOutput.From(solution).Projects)
        {
            output.WriteLine(project.Name);
            foreach (var target in project.Targets)
            {
                output.WriteLine($"  {target.Version}");
                foreach (var package in target.Packages) WritePackage(package, 4);
            }
        }

        output.WriteLine($@"Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}");

        void WritePackage(PackageOutput package, int padding)
        {
            output.WriteLine($"{new string(' ', padding)}{package.Name}: {package.Version}");
            if (package.Packages is null) return;
            foreach (var dependency in package.Packages) WritePackage(dependency, padding + 2);
        }
    }

    public void WriteErrors(TextWriter output, IEnumerable<AnalysisError> errors)
    {
        output.WriteLine("Errors:");
        foreach (var error in errors) output.WriteLine($"  {error.Message}");
    }
}
