using System.Collections.Generic;

namespace Automatron;

#if NET6_0
internal record TaskModel(IEnumerable<Task> Tasks, IEnumerable<Parameter> Parameters);
#endif