using System.IO.Enumeration;
using System.Text.RegularExpressions;

namespace DotNetWhere.Core.Search;

public sealed class SearchCriteria
{
    private readonly Func<string, bool> _name;
    private readonly Func<string, bool> _version;
    private readonly Func<string, bool> _target;

    public SearchCriteria(string? name, string? version, string? target)
    {
        _name = Compile(name);
        _version = Compile(version);
        _target = Compile(target);
        HasPackageFilter = !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(version);
    }

    public bool HasPackageFilter { get; }
    public bool MatchesPackage(string name, string version) => _name(name) && _version(version);
    public bool MatchesTarget(string target) => _target(target);

    private static Func<string, bool> Compile(string? expression)
    {
        if (string.IsNullOrEmpty(expression)) return _ => true;

        var negate = false;
        var pattern = expression;
        while (pattern.StartsWith('!'))
        {
            negate = !negate;
            pattern = pattern[1..];
        }

        if (pattern.Length == 0)
            throw new ArgumentException("A negated search expression must contain a pattern.");

        Func<string, bool> match;
        if (pattern.StartsWith('/'))
        {
            var regex = new Regex(pattern[1..], RegexOptions.None, TimeSpan.FromSeconds(1));
            match = regex.IsMatch;
        }
        else if (pattern.Contains('*') || pattern.Contains('?'))
        {
            match = value => FileSystemName.MatchesSimpleExpression(pattern, value);
        }
        else
        {
            match = value => value.Equals(pattern, StringComparison.InvariantCultureIgnoreCase);
        }

        return negate ? value => !match(value) : match;
    }
}
