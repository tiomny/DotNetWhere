using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Providers;

public interface IProvider
{
    Response Get(Request request);
}