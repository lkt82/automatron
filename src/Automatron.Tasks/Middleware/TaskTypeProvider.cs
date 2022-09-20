#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.Models;
using Automatron.Reflection;

namespace Automatron.Tasks.Middleware;

internal class TaskTypeProvider : ITypeProvider
{
    public TaskTypeProvider()
    {
        Types = Assembly.GetEntryAssembly()!.GetTypes()
            .Where(c => !c.IsAbstract && !c.IsInterface && c.IsVisible)
            .Where(c => c.Accept(new TaskTypeVisitor())).ToArray();
    }

    public IEnumerable<Type> Types { get; }
}
#endif