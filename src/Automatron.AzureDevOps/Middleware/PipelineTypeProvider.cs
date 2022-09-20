#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class PipelineTypeProvider : ITypeProvider
{
    public PipelineTypeProvider()
    {
        Types = Assembly.GetEntryAssembly()!.GetTypes()
            .Where(c => !c.IsAbstract && !c.IsInterface && c.IsVisible)
            .Where(c => c.Accept(new PipelineTypeVisitor())).ToArray();
    }

    public IEnumerable<Type> Types { get; }
}
#endif