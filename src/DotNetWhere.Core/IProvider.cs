namespace DotNetWhere.Core;

public interface IProvider
{
    Response Get(Request request);
}