#if NET8_0
using System.Threading;

namespace Automatron.Models;

public interface IAction
{
    public object? Invoke(object service, CancellationToken cancellationToken);
}
#endif