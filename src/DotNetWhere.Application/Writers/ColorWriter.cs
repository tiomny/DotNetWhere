using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Writers;

internal class ColorWriter(TextWriter writer) : WriterBase(writer)
{
    public override void WriteSolution(Solution solution, ElapsedTime elapsedTime)
    {
        var root = new Tree(solution.Name);

        AnsiConsole.Live(root)
            .Start(ctx =>
            {
                foreach (var project in solution.Projects)
                {
                    var projectNode = root.AddNode(project.Name);
                    ctx.Refresh();

                    foreach (var target in project.Targets)
                    {
                        var targetNode = projectNode.AddNode(target.Version);
                        ctx.Refresh();
                        foreach (var package in target.Packages)
                        {
                            WritePackage(targetNode, package!);
                            ctx.Refresh();
                        }
                    }
                }
            });

        AnsiConsole.Markup($@"Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}".Bold());
    }
    public override void WriteErrors(IEnumerable<string> errors)
    {
        AnsiConsole.Markup("Errors:".Bold());
        foreach (var error in errors)
            AnsiConsole.Markup(error.Red());
        AnsiConsole.WriteLine();
    }

    private void WritePackage(TreeNode node, Package package)
    {
        var p = $"{package.Name}: {package.Version.DarkCyan()}";
        if (package.IsMatch)
        {
            p = p.Green();
        }
        var packageNode = node.AddNode(p);
        if (package.Packages is not null)
        {
            foreach (var dep in package.Packages)
            {
                WritePackage(packageNode, dep);
            }
        }
    }
}
