using System.Text.Json;

using DotNetWhere.Application;
using DotNetWhere.Core.Graph;
using DotNetWhere.Rendering;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DotNetWhere.Tests;

public sealed class CliTests
{
    [Theory]
    [InlineData("Compact")]
    [InlineData("Color")]
    [InlineData("Json")]
    public async Task Run_SupportedFormats_HonorProvidedOutput(string format)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CreateCli().RunAsync(["--format", format], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Root", output.ToString());
        Assert.Empty(error.ToString());
        output.Write("still open");
        error.Write("still open");
    }

    [Theory]
    [InlineData("Compact")]
    [InlineData("Color")]
    [InlineData("Json")]
    public async Task Run_OutputFile_AllFormatsWriteToFile(string format)
    {
        using var directory = new TestDirectory();
        var path = System.IO.Path.Combine(directory.Path, "output with spaces.txt");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CreateCli().RunAsync(["--format", format, "--output", path], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Root", File.ReadAllText(path));
        Assert.Empty(output.ToString());
        Assert.Empty(error.ToString());
        using var exclusive = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [Fact]
    public async Task Run_Json_PreservesExistingOutputShape()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        await CreateCli().RunAsync(["--format", "Json", "-p", "Root"], output, error);

        using var document = JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal("fixture", root.GetProperty("solution").GetProperty("Name").GetString());
        Assert.True(TimeSpan.TryParse(root.GetProperty("elapsedTime").GetString(), out _));
        var project = root.GetProperty("solution").GetProperty("Projects")[0];
        Assert.Equal("Project", project.GetProperty("Name").GetString());
        Assert.Equal("1.0.0", project.GetProperty("Version").GetString());
        var target = project.GetProperty("Targets")[0];
        Assert.Equal("net10.0", target.GetProperty("Version").GetString());
        var package = target.GetProperty("Packages")[0];
        Assert.Equal("Root", package.GetProperty("Name").GetString());
        Assert.Equal("1.2.0", package.GetProperty("Version").GetString());
        Assert.True(package.GetProperty("IsMatch").GetBoolean());
        Assert.False(package.TryGetProperty("Packages", out _));
    }

    [Fact]
    public async Task Run_AnalysisFailure_ReturnsNonzeroAndJsonErrorEnvelope()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CreateCli().RunAsync(["--format", "Json", "-p", "Missing"], output, error);

        Assert.Equal(1, exitCode);
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("Package not found.", document.RootElement.GetProperty("errors")[0].GetString());
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task Run_InformationalOptions_DoNotExecuteAnalysis(string option)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var cli = new Cli(new AnalyzeDependencies(new RejectingSource()), new OutputFormatterFactory());

        var exitCode = await cli.RunAsync([option], output, error);

        Assert.Equal(0, exitCode);
        Assert.NotEmpty(output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Run_InvalidArguments_ReturnsUsageError()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CreateCli().RunAsync(["--unknown"], output, error);

        Assert.Equal(2, exitCode);
        Assert.NotEmpty(error.ToString());
    }

    [Fact]
    public async Task Run_UnsupportedYaml_DoesNotLoadGraphOrTruncateFile()
    {
        using var directory = new TestDirectory();
        var path = System.IO.Path.Combine(directory.Path, "existing.txt");
        File.WriteAllText(path, "preserve");
        using var output = new StringWriter();
        using var error = new StringWriter();
        var cli = new Cli(new AnalyzeDependencies(new RejectingSource()), new OutputFormatterFactory());

        var exitCode = await cli.RunAsync(["--format", "Yaml", "-o", path], output, error);

        Assert.Equal(2, exitCode);
        Assert.Contains("Yaml", error.ToString());
        Assert.Equal("preserve", File.ReadAllText(path));
    }

    [Fact]
    public async Task Run_InvalidFilter_PreservesExistingOutputFile()
    {
        using var directory = new TestDirectory();
        var path = System.IO.Path.Combine(directory.Path, "existing.txt");
        File.WriteAllText(path, "preserve");
        using var output = new StringWriter();
        using var error = new StringWriter();
        var cli = new Cli(new AnalyzeDependencies(new RejectingSource()), new OutputFormatterFactory());

        var exitCode = await cli.RunAsync(["--format", "Json", "-p", "/[", "-o", path], output, error);

        Assert.Equal(1, exitCode);
        Assert.Equal("preserve", File.ReadAllText(path));
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(1, document.RootElement.GetProperty("errors").GetArrayLength());
    }

    [Fact]
    public async Task Run_OutputFailure_ReturnsNonzero()
    {
        using var directory = new TestDirectory();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CreateCli().RunAsync(["-o", directory.Path], output, error);

        Assert.Equal(1, exitCode);
        Assert.NotEmpty(error.ToString());
    }

    [Fact]
    public async Task Run_Color_EscapesMarkupInNamesAndErrors()
    {
        var graph = new DependencyGraph("fixture[unknown]", [new ProjectGraph("Project[unknown]", "1.0.0",
            [new TargetGraph("net10.0", ["Root[unknown]"], [new ResolvedPackage("Root[unknown]", "1.2.0", [])])])]);
        var cli = new Cli(new AnalyzeDependencies(new FixedSource(graph)), new OutputFormatterFactory());
        using var output = new StringWriter();
        using var error = new StringWriter();

        var success = await cli.RunAsync([], output, error);
        var failure = await new Cli(new AnalyzeDependencies(new FailingSource()), new OutputFormatterFactory())
            .RunAsync([], output, error);

        Assert.Equal(0, success);
        Assert.Equal(1, failure);
        Assert.Contains("Root[unknown]", output.ToString());
        Assert.Contains("failure[unknown]", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public async Task Run_Cancelled_ReturnsCancellationCode()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exitCode = await CreateCli().RunAsync([], output, error, cancellation.Token);

        Assert.Equal(130, exitCode);
    }

    [Fact]
    public void CompositionRoot_ValidatesAndResolvesWithoutInvocationData()
    {
        using var provider = CompositionRoot.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<Cli>());
    }

    [Fact]
    public void InnerLayers_DoNotReferenceInfrastructureOrPresentation()
    {
        foreach (var assembly in new[] { typeof(DependencyGraph).Assembly, typeof(AnalyzeDependencies).Assembly })
        {
            Assert.DoesNotContain(assembly.GetReferencedAssemblies(), reference =>
                reference.Name!.StartsWith("NuGet.", StringComparison.Ordinal) ||
                reference.Name.StartsWith("Microsoft.Extensions.DependencyInjection", StringComparison.Ordinal) ||
                reference.Name is "CommandLine" or "Spectre.Console" or "Newtonsoft.Json" or
                    "DotNetWhere.Infrastructure" or "DotNetWhere");
        }
    }

    private static Cli CreateCli() => new(new AnalyzeDependencies(new FixedSource(new DependencyGraph("fixture",
        [new ProjectGraph("Project", "1.0.0", [new TargetGraph("net10.0", ["Root"],
            [new ResolvedPackage("Root", "1.2.0", [])])])]))), new OutputFormatterFactory());

    private sealed class FixedSource(DependencyGraph graph) : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            Task.FromResult(graph);
    }

    private sealed class RejectingSource : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Analysis must not execute.");
    }

    private sealed class FailingSource : IDependencyGraphSource
    {
        public Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken) =>
            throw new GraphSourceException(AnalysisErrorCode.ProcessFailed, "failure[unknown]");
    }
}
