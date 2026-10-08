namespace DotNetWhere.Infrastructure;

public sealed record DotNetProcessOptions(string Executable = "dotnet", TimeSpan? Timeout = null);
