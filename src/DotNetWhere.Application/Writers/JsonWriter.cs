using DotNetWhere.Core.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotNetWhere.Application.Writers;

internal sealed class JsonWriter : WriterBase, IDisposable
{
    private readonly JsonSerializer _jsonSerializer;
    private readonly JsonTextWriter _jsonTextWriter;

    public JsonWriter(TextWriter writer) : base(writer)
    {
        _jsonSerializer = new JsonSerializer();
        _jsonSerializer.Converters.Add(new JavaScriptDateTimeConverter());
        _jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

        _jsonTextWriter = new JsonTextWriter(writer);
    }

    public override void WriteErrors(IEnumerable<string> errors) =>
        WriteObject(new { errors });

    public override void WriteSolution(Solution solution, ElapsedTime elapsedTime) =>
        WriteObject(new { solution, elapsedTime });

    private void WriteObject(object obj) =>
        _jsonSerializer.Serialize(_jsonTextWriter, obj);

    public void Dispose() =>
        ((IDisposable)_jsonTextWriter).Dispose();
}
