namespace DotNetWhere.Rendering;

internal sealed class OutputFormatterFactory
{
    public IOutputFormatter Create(OutputFormat format) => format switch
    {
        OutputFormat.Color => new ColorFormatter(),
        OutputFormat.Compact => new CompactFormatter(),
        OutputFormat.Json => new JsonFormatter(),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported output format.")
    };
}
