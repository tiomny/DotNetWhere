using System;

using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Loggers;

internal class CompactOutputWriter : IOutputWriter
{
    public static readonly string Contract = OutputFormat.Compact.ToString();
    public void Log(Response response)
    {
        WriteLine();

        if (response.IsSuccess)
            Log(response.Solution);
        else
            LogErrors(response.Errors);
    }

    public void Log(ElapsedTime elapsedTime) =>
        WriteLine(message: $@"Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}");

    public Response LogAction(Func<Response> getResponse) =>
        getResponse();

    public void LogErrors(IEnumerable<string> errors)
    {
        WriteLine(message: "Errors:");
        foreach (var error in errors)
            WriteLine(2, error);
    }

    private void Log(Solution? solution)
    {
        if (solution is null)
        {
            LogErrors(new[] { "Solution not found" });
            return;
        }

        foreach (var project in solution.Projects)
        {
            WriteLine(message: project.Name);
            foreach (var target in project.Targets)
            {
                WriteLine(2, target.Version);
                foreach (var package in target.Packages)
                {
                    LogPackage(4, package!);
                }
            }
        }
    }

    private void LogPackage(int pad, Package package)
    {
        WriteLine(pad, package.ToString());
        if (package.Packages is not null)
        {
            foreach (var dep in package.Packages)
            {
                LogPackage(pad + 2, dep);
            }
        }
    }

    private void WriteLine(int pad = 0, string? message = null) =>
        Console.WriteLine("".PadLeft(pad) + message);
}
