using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Loggers;

internal sealed class ColorLogger(IAnsiConsole console) : ILogger
{
    private readonly IDictionary<int, Func<string, FormattableString>> _headerLabelsColors =
        new Dictionary<int, Func<string, FormattableString>>
        {
            {1, Extensions.DarkGreen},
            {2, Extensions.Green},
            {3, Extensions.DarkCyan}
        };

    public Response LogAction(Func<Response> getResponse) =>
        console
            .Status()
            .Spinner(Spinner.Known.Line)
            .SpinnerStyle(new Style(Color.Green))
            .Start("Analyzing...", _ => getResponse());

    public void Log(Response response)
    {
        console.WriteLine();

        if (response.IsSuccess)
            Log(response.Solution);
        else
            LogErrors(response.Errors);
    }

    public void Log(ElapsedTime elapsedTime) =>
        console.MarkupLineInterpolated($@"Time elapsed: {elapsedTime:hh\:mm\:ss\.ff}");

    private void Log(Solution node)
    {
        /*
        var maxWidth = Console.WindowWidth - Tabs.Double;
        var nameWidth = GetLongestNodeNameLength(node);
        var rootLastNodesSum = node.LastNodesSum;

        console.MarkupLineInterpolated("Answer:".Bold());

        console.MarkupLineInterpolated(GetHeaderLabel(1, node, nameWidth));
        foreach (var project in node.Nodes)
        {
            console.MarkupLineInterpolated(GetHeaderLabel(2, project, nameWidth, rootLastNodesSum));
            foreach (var target in project.Nodes)
            {
                console.MarkupLineInterpolated(GetHeaderLabel(3, target, nameWidth, rootLastNodesSum));

                var index = 0;

                var paths = target.Nodes.SelectMany(GetPaths);
                foreach (var path in paths)
                {
                    var indexWidth = target.LastNodesSum.ToString().Length < Tabs.Double
                        ? Tabs.Double
                        : Tabs.Triple;

                    var width = indexWidth;

                    console.MarkupInterpolated($"{$"{++index}.".PadRight(indexWidth)}");

                    for (var iterator = 0; iterator < path.Count; iterator++)
                    {
                        var isLastItem = iterator == path.Count - 1;
                        var item = path[iterator];
                        var itemWidth = item.Length + (isLastItem ? 0 : 3);
                        width += itemWidth;

                        if (width >= maxWidth)
                        {
                            var nextLineTabWidth = indexWidth + Tabs.Single;
                            width = nextLineTabWidth + itemWidth;
                            console.WriteLine();
                            console.Write(string.Empty.PadRight(nextLineTabWidth));
                        }

                        console.MarkupInterpolated(item.Contains(request.PackageName) ? item.Red() : $"{item}");

                        if (!isLastItem)
                            console.MarkupInterpolated($" {Characters.Separator} ");
                    }

                    console.WriteLine();
                }
            }

            console.WriteLine();
        }
        */
    }

    public void LogErrors(IEnumerable<string> errors)
    {
        console.MarkupLineInterpolated("Errors:".Bold());
        foreach (var error in errors)
            console.MarkupLineInterpolated(error.Red());
        console.WriteLine();
    }

    private static class Characters
    {
        public const char Level = '*';
        public const char Separator = '>';
    }

    private static class Tabs
    {
        public const int Single = 2;
        public const int Double = 4;
        public const int Triple = 6;
    }
}