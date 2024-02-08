using System;

using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Loggers;

internal class CompactOutputWriter : IOutputWriter
{
    public static readonly string Contract = OutputFormat.Compact.ToString();
    public void Log(Response response)
    {
        Console.WriteLine();

        if (response.IsSuccess)
            Log(response.Solution);
        else
            LogErrors(response.Errors);
    }

    public void Log(ElapsedTime elapsedTime) { }
    public Response LogAction(Func<Response> getResponse) => getResponse();
    public void LogErrors(IEnumerable<string> errors)
    {
        Console.WriteLine("Errors:");
        foreach (var error in errors)
            Console.WriteLine(error);
        Console.WriteLine();
    }

    private void Log(Solution? solution)
    {
        if (solution is null)
        {
            Console.WriteLine("Nothing was found");
            return;
        }

        foreach (var project in solution.Projects)
        {
            WriteLine(0, project.Name);
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

    private void LogPackage(int shift, Package package)
    {
        WriteLine(shift, package.ToString());
        foreach (var dep in package.Packages)
        {
            LogPackage(shift + 2, dep);
        }
    }

    private void WriteLine(int shift, string message) =>
        //Console.WriteLine($"{{{shift}}}", message);
        Console.WriteLine("".PadLeft(shift) + message);

}
