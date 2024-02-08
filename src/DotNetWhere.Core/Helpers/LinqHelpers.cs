namespace DotNetWhere.Core.Helpers;

internal static class LinqHelpers
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) =>
        enumerable
        .Where(t => t != null)
        .Select(t => t!);
}
