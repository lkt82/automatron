#if NET6_0
namespace Automatron.Models;

public interface IAction
{
    public object? Invoke(object service);
}
#endif