namespace DotNetWhere.Core.Results;

internal interface IResultHandler<T>
{
    Result<T> Handle();
}