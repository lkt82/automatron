namespace Automatron.Models;

#if NET6_0
public interface IAction
{
    public object? Invoke(object service);
}
#endif