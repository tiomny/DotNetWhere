using System.ComponentModel;

namespace DotNetWhere.Application;

[DefaultValue(Color)]
public enum OutputFormat
{
    Color,
    Compact,
    Yaml,
    Json
}
