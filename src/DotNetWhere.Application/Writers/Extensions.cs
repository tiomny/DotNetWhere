namespace DotNetWhere.Application.Writers;

internal static class Extensions
{
    public static string Bold(this string value) => $"[bold]{value}[/]";
    public static string DarkCyan(this string value) => $"[teal]{value}[/]";
    public static string DarkGreen(this string value) => $"[green]{value}[/]";
    public static string Green(this string value) => $"[lime]{value}[/]";
    public static string Red(this string value) => $"[red]{value}[/]";
}