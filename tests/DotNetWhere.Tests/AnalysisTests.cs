using DotNetWhere.Application;
using DotNetWhere.Core.Graph;

using Xunit;

namespace DotNetWhere.Tests;

public sealed class AnalysisTests
{
    [Fact]
    public async Task Execute_DifferentRequests_UsesEachRequestsFilters()
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));

        var first = await analysis.ExecuteAsync(new AnalysisRequest("first", PackageName: "Alpha"));
        var second = await analysis.ExecuteAsync(new AnalysisRequest("second", PackageName: "Beta"));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal("Alpha", Assert.Single(Assert.Single(first.Solution!.Projects).Targets[0]
            .Packages[0].Packages!).Name);
        Assert.Equal("Beta", Assert.Single(Assert.Single(second.Solution!.Projects).Targets[0]
            .Packages[0].Packages!).Name);
    }

    [Theory]
    [InlineData("alpha", "Alpha")]
    [InlineData("Al*", "Alpha")]
    [InlineData("Alph?", "Alpha")]
    [InlineData("/^Al", "Alpha")]
    [InlineData("!Beta", "Alpha")]
    [InlineData("!/^(Root|Beta)$", "Alpha")]
    [InlineData("!!Beta", "Beta")]
    public async Task Execute_SearchExpressions_PreservesPathsToMatches(string expression, string expected)
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));

        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture", PackageName: expression));

        Assert.True(result.IsSuccess);
        var root = Assert.Single(Assert.Single(Assert.Single(result.Solution!.Projects).Targets).Packages);
        Assert.Equal("Root", root.Name);
        var match = Assert.Single(root.Packages!);
        Assert.Equal(expected, match.Name);
        Assert.True(match.IsMatch);
    }

    [Theory]
    [InlineData("2.0.0")]
    [InlineData("2.*")]
    [InlineData("/^2\\.")]
    [InlineData("!3.0.0")]
    public async Task Execute_NameAndVersion_CombinesFilters(string version)
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));

        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture", "Alpha", version, "net10.*"));

        Assert.True(result.IsSuccess);
        var root = result.Solution!.Projects[0].Targets[0].Packages[0];
        Assert.Equal("2.0.0", Assert.Single(root.Packages!).Version);
        Assert.False(root.IsMatch);
    }

    [Theory]
    [InlineData("!")]
    [InlineData("!!")]
    [InlineData("/[")]
    public async Task Execute_InvalidFilter_DoesNotLoadGraph(string expression)
    {
        var analysis = new AnalyzeDependencies(new RejectingGraphSource());

        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture", PackageName: expression));

        Assert.False(result.IsSuccess);
        Assert.Equal(AnalysisErrorCode.InvalidFilter, Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task Execute_NoMatches_ReturnsDistinctError()
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));
        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture", "Missing"));

        Assert.Equal(AnalysisErrorCode.PackageNotFound, Assert.Single(result.Errors).Code);
        Assert.Null(result.Solution);
    }

    [Fact]
    public async Task Execute_NoPackageFilter_IncludesAllPackagesWithoutHighlighting()
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));
        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture"));

        var root = Assert.Single(Assert.Single(Assert.Single(result.Solution!.Projects).Targets).Packages);
        Assert.Equal(2, root.Packages!.Count);
        Assert.False(root.IsMatch);
        Assert.All(root.Packages, package => Assert.False(package.IsMatch));
    }

    [Fact]
    public async Task Execute_SharedNamesAcrossProjectsAndTargets_KeepsGraphsIndependent()
    {
        var graph = new DependencyGraph("fixture",
        [
            new ProjectGraph("First", "1.0.0", [Target("net10.0", "Alpha", "2.0.0")]),
            new ProjectGraph("Second", "1.0.0",
                [Target("net10.0", "Beta", "3.0.0"), Target("net8.0", "Gamma", "4.0.0")])
        ]);
        var analysis = new AnalyzeDependencies(new FixedGraphSource(graph));

        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture"));

        var projects = result.Solution!.Projects;
        Assert.Equal("Alpha", Assert.Single(projects[0].Targets[0].Packages[0].Packages!).Name);
        Assert.Equal("Beta", Assert.Single(projects[1].Targets[0].Packages[0].Packages!).Name);
        Assert.Equal("Gamma", Assert.Single(projects[1].Targets[1].Packages[0].Packages!).Name);
    }

    [Fact]
    public async Task Execute_ConcurrentRequests_DoNotShareSearchState()
    {
        var analysis = new AnalyzeDependencies(new FixedGraphSource(CreateGraph()));
        var requests = Enumerable.Range(0, 16)
            .Select(index => Task.Run(() => analysis.ExecuteAsync(
                new AnalysisRequest("fixture", PackageName: index % 2 == 0 ? "Alpha" : "Beta"))));

        var results = await Task.WhenAll(requests);

        for (var index = 0; index < results.Length; index++)
        {
            Assert.Equal(index % 2 == 0 ? "Alpha" : "Beta",
                Assert.Single(results[index].Solution!.Projects[0].Targets[0].Packages[0].Packages!).Name);
        }
    }

    [Fact]
    public async Task Execute_Cancelled_PropagatesCancellation()
    {
        var analysis = new AnalyzeDependencies(new RejectingGraphSource());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            analysis.ExecuteAsync(new AnalysisRequest("fixture"), cancellation.Token));
    }

    [Fact]
    public async Task Execute_LoadFailure_PreservesStructuredErrorAndSemicolons()
    {
        var analysis = new AnalyzeDependencies(new FailingGraphSource());
        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture"));

        var error = Assert.Single(result.Errors);
        Assert.Equal(AnalysisErrorCode.ProcessFailed, error.Code);
        Assert.Equal("restore failed; details", error.Message);
    }

    [Fact]
    public async Task Execute_UnexpectedDefect_IsNotConvertedToNoMatches()
    {
        var analysis = new AnalyzeDependencies(new RejectingGraphSource());
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            analysis.ExecuteAsync(new AnalysisRequest("fixture")));
    }

    [Fact]
    public async Task Execute_CyclicGraph_ReturnsInvalidGraph()
    {
        var graph = new DependencyGraph("fixture", [new ProjectGraph("Project", "1.0.0",
            [new TargetGraph("net10.0", ["A"],
                [new ResolvedPackage("A", "1.0.0", ["B"]), new ResolvedPackage("B", "1.0.0", ["A"])])])]);
        var analysis = new AnalyzeDependencies(new FixedGraphSource(graph));

        var result = await analysis.ExecuteAsync(new AnalysisRequest("fixture"));

        Assert.Equal(AnalysisErrorCode.InvalidGraph, Assert.Single(result.Errors).Code);
    }

    private static DependencyGraph CreateGraph() => new("fixture",
        [new ProjectGraph("Project", "1.0.0", [new TargetGraph("net10.0", ["Root"],
        [
            new ResolvedPackage("Root", "1.0.0", ["Alpha", "Beta"]),
            new ResolvedPackage("Alpha", "2.0.0", []),
            new ResolvedPackage("Beta", "3.0.0", [])
        ])])]);

    private static TargetGraph Target(string framework, string dependency, string version) =>
        new(framework, ["Root"],
            [new ResolvedPackage("Root", "1.0.0", [dependency]), new ResolvedPackage(dependency, version, [])]);

    private sealed class FixedGraphSource(DependencyGraph graph) : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            Task.FromResult(graph);
    }

    private sealed class RejectingGraphSource : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The graph should not have been loaded.");
    }

    private sealed class FailingGraphSource : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            throw new GraphSourceException(AnalysisErrorCode.ProcessFailed, "restore failed; details");
    }
}
