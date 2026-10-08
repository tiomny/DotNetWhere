using DotNetWhere.Application;
using DotNetWhere.Core.Models;

using Spectre.Console;

namespace DotNetWhere.Rendering;

internal sealed class ColorFormatter : IOutputFormatter
{
    public void WriteSolution(TextWriter output, Solution solution, TimeSpan elapsedTime)
    {
        var model = SolutionOutput.From(solution);
        var tree = new Tree(Markup.Escape(model.Name));
        foreach (var project in model.Projects)
        {
            var projectNode = tree.AddNode(Markup.Escape(project.Name));
            foreach (var target in project.Targets)
            {
                var targetNode = projectNode.AddNode(Markup.Escape(target.Version));
                foreach (var package in target.Packages) WritePackage(targetNode, package);
            }
        }

        var console = CreateConsole(output);
        console.Write(tree);
        console.MarkupLine($@"[bold]Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}[/]");
    }

    public void WriteErrors(TextWriter output, IEnumerable<AnalysisError> errors)
    {
        var console = CreateConsole(output);
        console.MarkupLine("[bold]Errors:[/]");
        foreach (var error in errors) console.MarkupLine($"[red]{Markup.Escape(error.Message)}[/]");
    }

    private static IAnsiConsole CreateConsole(TextWriter output) => AnsiConsole.Create(new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput(output),
        Interactive = InteractionSupport.No
    });

    private static void WritePackage(TreeNode parent, PackageOutput package)
    {
        var name = Markup.Escape(package.Name);
        var version = Markup.Escape(package.Version);
        var label = package.IsMatch ? $"[lime]{name}: {version}[/]" : $"{name}: [teal]{version}[/]";
        var node = parent.AddNode(label);
        if (package.Packages is null) return;
        foreach (var dependency in package.Packages) WritePackage(node, dependency);
    }
}
