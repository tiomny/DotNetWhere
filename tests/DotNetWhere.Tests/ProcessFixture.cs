using System.Diagnostics;

namespace DotNetWhere.Tests;

internal static class ProcessFixture
{
    public static async Task<int> Main(string[] args)
    {
        if (args[0] == "hold-output")
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return 0;
        }

        if (args[0] is "leave-child" or "wait-child")
        {
            using var child = Process.Start(new ProcessStartInfo(ExecutablePath)
            {
                UseShellExecute = false,
                ArgumentList = { "hold-output" }
            })!;
            File.WriteAllText("child-pid.txt", child.Id.ToString());
            if (args[0] == "wait-child") await Task.Delay(Timeout.InfiniteTimeSpan);
            return 0;
        }

        if (args[0] == "fail" || File.Exists("fail"))
        {
            Console.WriteLine("stdout failure; details");
            Console.Error.WriteLine("stderr failure");
            return 17;
        }

        if (args[0] == "hang" || File.Exists("hang"))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan);
        }

        if (args[0] == "msbuild")
        {
            var graphPath = args.Single(arg => arg.StartsWith("/p:RestoreGraphOutputPath="))
                ["/p:RestoreGraphOutputPath=".Length..];
            File.WriteAllText("graph-path.txt", graphPath);
            File.Copy("restore-graph.json", graphPath);
            if (File.Exists("hang-msbuild")) await Task.Delay(Timeout.InfiniteTimeSpan);
            if (File.Exists("fail-msbuild"))
            {
                Console.Error.WriteLine("graph generation failed");
                return 17;
            }
        }

        return 0;
    }

    public static string ExecutablePath => System.IO.Path.Combine(AppContext.BaseDirectory,
        OperatingSystem.IsWindows() ? "DotNetWhere.Tests.exe" : "DotNetWhere.Tests");

    public static async Task WaitForFileAsync(string directory, string name)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!File.Exists(System.IO.Path.Combine(directory, name)))
            await Task.Delay(10, deadline.Token);
    }
}
