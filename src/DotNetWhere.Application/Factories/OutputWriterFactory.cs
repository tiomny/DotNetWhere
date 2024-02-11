namespace DotNetWhere.Application.Factories;

internal sealed class OutputWriterFactory(Options options) : IFactory<TextWriter>, IDisposable
{
    private readonly List<TextWriter> _created = [];
#pragma warning disable IDISP015 // Member should not return created and cached instance
    public TextWriter Create()
#pragma warning restore IDISP015 // Member should not return created and cached instance
    {
        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            var textWriter = new StreamWriter(options.OutputFile);
            _created.Add(textWriter);
            return textWriter;
        }

        return Console.Out;
    }

    public void Dispose() =>
        _created.ForEach(x => x.Dispose());
}
