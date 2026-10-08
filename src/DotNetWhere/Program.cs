using DotNetWhere;

using Microsoft.Extensions.DependencyInjection;

using var provider = CompositionRoot.BuildServiceProvider();
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += Cancel;
try
{
    return await provider.GetRequiredService<Cli>().RunAsync(args, Console.Out, Console.Error, cancellation.Token);
}
finally
{
    Console.CancelKeyPress -= Cancel;
}

void Cancel(object? sender, ConsoleCancelEventArgs eventArgs)
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
}
