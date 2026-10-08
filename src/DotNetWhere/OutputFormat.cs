using System.ComponentModel;

namespace DotNetWhere;

[DefaultValue(Color)]
public enum OutputFormat
{
    Color,
    Compact,
    Yaml,
    Json
}
