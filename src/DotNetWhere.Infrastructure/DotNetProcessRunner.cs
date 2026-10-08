using System.ComponentModel;
using System.Diagnostics;

using DotNetWhere.Application;

namespace DotNetWhere.Infrastructure;

public sealed class DotNetProcessRunner(DotNetProcessOptions options)
{
    public async Task RunAsync(
        string directory,
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startInfo = new ProcessStartInfo(options.Executable)
        {
            WorkingDirectory = directory,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Win32Exception exception)
        {
            throw new GraphSourceException(AnalysisErrorCode.ProcessFailed,
                $"Could not start '{options.Executable}': {exception.Message}", exception);
        }

        using var timeout = new CancellationTokenSource(options.Timeout ?? TimeSpan.FromSeconds(120));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var output = process.StandardOutput.ReadToEndAsync(cancellation.Token);
        var error = process.StandardError.ReadToEndAsync(cancellation.Token);
        try
        {
            await process.WaitForExitAsync(cancellation.Token);
            await Task.WhenAll(output, error).WaitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (process.HasExited)
            {
                // The process exited between checking its state and sending the kill signal.
            }

            await process.WaitForExitAsync(CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();
            throw new GraphSourceException(AnalysisErrorCode.ProcessFailed,
                $"'{options.Executable}' process timed out.");
        }

        var standardOutput = await output;
        var standardError = await error;
        if (process.ExitCode != 0)
        {
            throw new GraphSourceException(AnalysisErrorCode.ProcessFailed,
                $"'{options.Executable}' exited with code {process.ExitCode}.{Environment.NewLine}" +
                standardOutput + standardError);
        }
    }
}
