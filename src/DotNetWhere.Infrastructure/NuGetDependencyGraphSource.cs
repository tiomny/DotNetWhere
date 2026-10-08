using DotNetWhere.Application;
using DotNetWhere.Core.Graph;

namespace DotNetWhere.Infrastructure;

public sealed class NuGetDependencyGraphSource(DotNetProcessRunner runner, AssetsGraphReader reader)
    : IDependencyGraphSource
{
    public async Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string workingDirectory;
        try
        {
            workingDirectory = Path.GetFullPath(directory);
            ValidateDirectory(workingDirectory);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new GraphSourceException(AnalysisErrorCode.InvalidDirectory, exception.Message, exception);
        }

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "dotnetwhere-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(temporaryDirectory);
            var graphPath = Path.Combine(temporaryDirectory, "restore-graph.json");

            await runner.RunAsync(workingDirectory, ["restore", workingDirectory], cancellationToken);
            await runner.RunAsync(workingDirectory,
                ["msbuild", workingDirectory, "/t:GenerateRestoreGraphFile", $"/p:RestoreGraphOutputPath={graphPath}"],
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            return reader.Read(graphPath, new DirectoryInfo(workingDirectory).Name);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new GraphSourceException(AnalysisErrorCode.InvalidAssets, exception.Message, exception);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory)) Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static void ValidateDirectory(string directory)
    {
        var containsInput = Directory.EnumerateFiles(directory).Any(path =>
            Path.GetExtension(path).Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(path).Equals(".sln", StringComparison.OrdinalIgnoreCase));
        if (!containsInput)
            throw new GraphSourceException(AnalysisErrorCode.InvalidDirectory,
                $"Directory '{directory}' does not contain a C# project or solution.");
    }
}
