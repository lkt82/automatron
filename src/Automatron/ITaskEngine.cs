#if NET6_0
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automatron;

internal interface ITaskEngine
{
    public Task<int> RunAsync(IEnumerable<string> tasks, IEnumerable<string>? skip = null, bool skipAll = false);
}
#endif