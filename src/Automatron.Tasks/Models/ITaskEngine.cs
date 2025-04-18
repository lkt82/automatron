﻿#if NET8_0

using System.Collections.Generic;
using System.Threading;

namespace Automatron.Tasks.Models;

public interface ITaskEngine
{
    public System.Threading.Tasks.Task Run(Task task, IEnumerable<ParameterValue>? parameters, CancellationToken cancellationToken);
}
#endif