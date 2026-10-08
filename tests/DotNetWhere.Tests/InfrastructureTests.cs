using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

using DotNetWhere.Application;
using DotNetWhere.Infrastructure;

using Xunit;

namespace DotNetWhere.Tests;

public sealed class InfrastructureTests
{
    [Fact]
    public void Read_SharedPackagesAcrossProjectsAndTargets_PreservesEachResolvedVersion()
    {
        using var directory = new TestDirectory();
        var firstDirectory = System.IO.Path.Combine(directory.Path, "First");
        var secondDirectory = System.IO.Path.Combine(directory.Path, "Second");
        var firstPath = WriteGraph(firstDirectory);
        var secondPath = WriteGraph(secondDirectory);
        var first = JsonNode.Parse(File.ReadAllText(firstPath))!;
        var second = JsonNode.Parse(File.ReadAllText(secondPath))!;
        var projectPath = System.IO.Path.Combine(secondDirectory, "Project.csproj");
        var project = second["projects"]![projectPath]!;
        project["restore"]!["projectName"] = "Second";
        project["frameworks"]!["net8.0"] = project["frameworks"]!["net10.0"]!.DeepClone();
        var assetsPath = System.IO.Path.Combine(secondDirectory, "obj", "project.assets.json");
        var assets = JsonNode.Parse(File.ReadAllText(assetsPath))!;
        var framework = assets["targets"]!["net10.0"]!.AsObject();
        framework["Dependency/4.0.0"] = framework["Dependency/2.7.0"]!.DeepClone();
        framework.Remove("Dependency/2.7.0");
        framework["Root/1.2.0"]!["dependencies"]!["Dependency"] = "[2.0.0, 9.0.0)";
        var otherFramework = framework.DeepClone().AsObject();
        otherFramework["Dependency/5.0.0"] = otherFramework["Dependency/4.0.0"]!.DeepClone();
        otherFramework.Remove("Dependency/4.0.0");
        assets["targets"]!["net8.0"] = otherFramework;
        File.WriteAllText(assetsPath, assets.ToJsonString());
        first["projects"]![projectPath] = project.DeepClone();
        File.WriteAllText(firstPath, first.ToJsonString());

        var graph = new AssetsGraphReader().Read(firstPath, "fixture");

        Assert.Equal(2, graph.Projects.Count);
        Assert.Equal("2.7.0", graph.Projects[0].Targets[0].Packages["Dependency"].Version);
        var secondProject = graph.Projects.Single(candidate => candidate.Name == "Second");
        Assert.Equal("4.0.0", secondProject.Targets.Single(target => target.Framework == "net10.0")
            .Packages["Dependency"].Version);
        Assert.Equal("5.0.0", secondProject.Targets.Single(target => target.Framework == "net8.0")
            .Packages["Dependency"].Version);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Load_ProcessFailureOrCancellation_RemovesTemporaryGraph(bool cancel)
    {
        using var directory = new TestDirectory();
        WriteGraph(directory.Path);
        File.WriteAllText(System.IO.Path.Combine(directory.Path, cancel ? "hang-msbuild" : "fail-msbuild"), "");
        using var cancellation = new CancellationTokenSource();

        if (cancel)
        {
            var loading = CreateSource().LoadAsync(directory.Path, cancellation.Token);
            await ProcessFixture.WaitForFileAsync(directory.Path, "graph-path.txt");
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                loading);
        }
        else
        {
            var exception = await Assert.ThrowsAsync<GraphSourceException>(() =>
                CreateSource().LoadAsync(directory.Path, cancellation.Token));
            Assert.Equal(AnalysisErrorCode.ProcessFailed, exception.Code);
        }

        var graphPath = File.ReadAllText(System.IO.Path.Combine(directory.Path, "graph-path.txt"));
        Assert.False(Directory.Exists(System.IO.Path.GetDirectoryName(graphPath)));
    }

    [Fact]
    public async Task Run_CancelledParent_StopsItsDescendants()
    {
        using var directory = new TestDirectory();
        var runner = new DotNetProcessRunner(new DotNetProcessOptions(ProcessFixture.ExecutablePath));
        using var cancellation = new CancellationTokenSource();
        var running = runner.RunAsync(directory.Path, ["wait-child"], cancellation.Token);
        await ProcessFixture.WaitForFileAsync(directory.Path, "child-pid.txt");
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);

        var processId = int.Parse(File.ReadAllText(System.IO.Path.Combine(directory.Path, "child-pid.txt")));
        try
        {
            using var child = Process.GetProcessById(processId);
            Assert.True(child.HasExited);
        }
        catch (ArgumentException)
        {
            // The terminated descendant has already been reaped.
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Run_ExitedParentWithOpenChildPipes_StillHonorsDeadline(bool cancel)
    {
        using var directory = new TestDirectory();
        var runner = new DotNetProcessRunner(new DotNetProcessOptions(ProcessFixture.ExecutablePath,
            cancel ? TimeSpan.FromSeconds(30) : TimeSpan.FromSeconds(1)));
        using var cancellation = new CancellationTokenSource();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (cancel)
            {
                var running = runner.RunAsync(directory.Path, ["leave-child"], cancellation.Token);
                await ProcessFixture.WaitForFileAsync(directory.Path, "child-pid.txt");
                stopwatch.Restart();
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
            }
            else
            {
                var exception = await Assert.ThrowsAsync<GraphSourceException>(() =>
                    runner.RunAsync(directory.Path, ["leave-child"], cancellation.Token));
                Assert.Contains("timed out", exception.Message);
            }

            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3));
            Assert.True(File.Exists(System.IO.Path.Combine(directory.Path, "child-pid.txt")));
        }
        finally
        {
            var path = System.IO.Path.Combine(directory.Path, "child-pid.txt");
            if (File.Exists(path))
            {
                try
                {
                    using var child = Process.GetProcessById(int.Parse(File.ReadAllText(path)));
                    if (!child.HasExited) child.Kill(entireProcessTree: true);
                }
                catch (ArgumentException)
                {
                    // The finite-lived child has already exited.
                }
            }
        }
    }

    [Fact]
    public void Read_VersionRange_UsesResolvedVersionForRootsAndEdges()
    {
        using var directory = new TestDirectory();
        var graphPath = WriteGraph(directory.Path);

        var graph = new AssetsGraphReader().Read(graphPath, "fixture");

        var target = Assert.Single(Assert.Single(graph.Projects).Targets);
        Assert.Equal("Root", Assert.Single(target.Roots));
        Assert.Equal("1.2.0", target.Packages["Root"].Version);
        Assert.Equal("2.7.0", target.Packages["Dependency"].Version);
        Assert.Equal("Dependency", Assert.Single(target.Packages["Root"].Dependencies));
    }

    [Fact]
    public void Read_MissingAssets_IsNotTreatedAsNoMatches()
    {
        using var directory = new TestDirectory();
        var graphPath = WriteGraph(directory.Path);
        File.Delete(System.IO.Path.Combine(directory.Path, "obj", "project.assets.json"));

        var exception = Assert.Throws<GraphSourceException>(() => new AssetsGraphReader().Read(graphPath, "fixture"));

        Assert.Equal(AnalysisErrorCode.InvalidAssets, exception.Code);
    }

    [Fact]
    public void Read_InvalidJson_ReturnsInvalidAssets()
    {
        using var directory = new TestDirectory();
        var graphPath = WriteGraph(directory.Path);
        File.WriteAllText(graphPath, "{ broken");

        var exception = Assert.Throws<GraphSourceException>(() => new AssetsGraphReader().Read(graphPath, "fixture"));

        Assert.Equal(AnalysisErrorCode.InvalidAssets, exception.Code);
    }

    [Fact]
    public async Task Run_FailedProcess_PreservesExitCodeAndBothOutputStreams()
    {
        using var directory = new TestDirectory();
        var runner = new DotNetProcessRunner(new DotNetProcessOptions(ProcessFixture.ExecutablePath));

        var exception = await Assert.ThrowsAsync<GraphSourceException>(() =>
            runner.RunAsync(directory.Path, ["fail"], CancellationToken.None));

        Assert.Equal(AnalysisErrorCode.ProcessFailed, exception.Code);
        Assert.Contains("17", exception.Message);
        Assert.Contains("stdout failure; details", exception.Message);
        Assert.Contains("stderr failure", exception.Message);
    }

    [Fact]
    public async Task Run_CancelledProcess_PropagatesCancellation()
    {
        using var directory = new TestDirectory();
        var runner = new DotNetProcessRunner(new DotNetProcessOptions(ProcessFixture.ExecutablePath));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            runner.RunAsync(directory.Path, ["hang"], cancellation.Token));
    }

    [Fact]
    public async Task Run_Timeout_ReturnsProcessError()
    {
        using var directory = new TestDirectory();
        var runner = new DotNetProcessRunner(new DotNetProcessOptions(
            ProcessFixture.ExecutablePath, TimeSpan.FromMilliseconds(300)));

        var exception = await Assert.ThrowsAsync<GraphSourceException>(() =>
            runner.RunAsync(directory.Path, ["hang"], CancellationToken.None));

        Assert.Equal(AnalysisErrorCode.ProcessFailed, exception.Code);
        Assert.Contains("timed out", exception.Message);
    }

    [Fact]
    public async Task Load_PathWithSpaces_LoadsGraphAndRemovesTemporaryFiles()
    {
        using var directory = new TestDirectory();
        var workingDirectory = System.IO.Path.Combine(directory.Path, "project with spaces");
        Directory.CreateDirectory(workingDirectory);
        WriteGraph(workingDirectory);
        var source = CreateSource();

        var graph = await source.LoadAsync(workingDirectory, CancellationToken.None);

        Assert.Single(graph.Projects);
        var graphPath = File.ReadAllText(System.IO.Path.Combine(workingDirectory, "graph-path.txt"));
        Assert.False(File.Exists(graphPath));
        Assert.False(Directory.Exists(System.IO.Path.GetDirectoryName(graphPath)));
    }

    [Fact]
    public async Task Load_BrokenAssets_RemovesTemporaryFilesOnFailure()
    {
        using var directory = new TestDirectory();
        WriteGraph(directory.Path);
        File.Delete(System.IO.Path.Combine(directory.Path, "obj", "project.assets.json"));

        await Assert.ThrowsAsync<GraphSourceException>(() =>
            CreateSource().LoadAsync(directory.Path, CancellationToken.None));

        var graphPath = File.ReadAllText(System.IO.Path.Combine(directory.Path, "graph-path.txt"));
        Assert.False(Directory.Exists(System.IO.Path.GetDirectoryName(graphPath)));
    }

    [Fact]
    public async Task Load_InvalidDirectory_ReturnsInvalidDirectory()
    {
        using var directory = new TestDirectory();

        var exception = await Assert.ThrowsAsync<GraphSourceException>(() =>
            CreateSource().LoadAsync(directory.Path, CancellationToken.None));

        Assert.Equal(AnalysisErrorCode.InvalidDirectory, exception.Code);
    }

    private static NuGetDependencyGraphSource CreateSource() => new(
        new DotNetProcessRunner(new DotNetProcessOptions(ProcessFixture.ExecutablePath)), new AssetsGraphReader());

    internal static string WriteGraph(string directory)
    {
        var projectPath = System.IO.Path.Combine(directory, "Project.csproj");
        var outputPath = System.IO.Path.Combine(directory, "obj");
        Directory.CreateDirectory(outputPath);
        File.WriteAllText(projectPath, "<Project />");

        var spec = new
        {
            version = "1.0.0",
            restore = new
            {
                projectUniqueName = projectPath,
                projectName = "Project",
                projectPath,
                outputPath,
                projectStyle = "PackageReference",
                originalTargetFrameworks = new[] { "net10.0" }
            },
            frameworks = new Dictionary<string, object>
            {
                ["net10.0"] = new
                {
                    dependencies = new Dictionary<string, object>
                    {
                        ["Root"] = new { target = "Package", version = "[1.0.0, )" }
                    }
                }
            }
        };
        var restoreGraph = new
        {
            format = 1,
            restore = new Dictionary<string, object> { [projectPath] = new { } },
            projects = new Dictionary<string, object> { [projectPath] = spec }
        };
        var graphPath = System.IO.Path.Combine(directory, "restore-graph.json");
        File.WriteAllText(graphPath, JsonSerializer.Serialize(restoreGraph));
        File.WriteAllText(System.IO.Path.Combine(outputPath, "project.assets.json"), """
            {
              "version": 3,
              "targets": {
                "net10.0": {
                  "Root/1.2.0": { "type": "package", "dependencies": { "Dependency": "[2.0.0, 3.0.0)" } },
                  "Dependency/2.7.0": { "type": "package" }
                }
              },
              "libraries": {},
              "projectFileDependencyGroups": { "net10.0": ["Root >= 1.0.0"] }
            }
            """);
        return graphPath;
    }
}
