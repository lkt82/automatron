#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class StepVisitor : MemberVisitor<IEnumerable<Step>>
{
    private readonly Job _job;

    private readonly Dictionary<string, string[]?> _dependsOnMap = new();

    public StepVisitor(Job job)
    {
        _job = job;
    }

    public override IEnumerable<Step>? VisitType(Type type)
    {
        var stepMap = new Dictionary<string, Step>();

        foreach (var methodInfo in type.GetAllMethods())
        {
            foreach (var step in methodInfo.Accept(this) ?? Enumerable.Empty<Step>())
            {
                stepMap.Add(step.Name, step);

                yield return step;
            }
        }

        foreach (var stepName in _dependsOnMap)
        {
            var step = stepMap[stepName.Key];

            if (stepName.Value == null)
            {
                continue;
            }
            foreach (var dependsOn in stepName.Value)
            {
                step.DependsOn.Add(stepMap[dependsOn]);
            }
        }
    }

    public override IEnumerable<Step>? VisitMethod(MethodInfo methodInfo)
    {
        var stepAttribute = methodInfo.GetAllCustomAttribute<StepAttribute>();

        if (stepAttribute != null)
        {
            yield return CreateStep(methodInfo, stepAttribute);
        }
    }

    private Step CreateStep(MethodInfo methodInfo, StepAttribute stepAttribute)
    {
        var name = !string.IsNullOrEmpty(stepAttribute.Name) ? stepAttribute.Name : methodInfo.Name;

        _dependsOnMap[name] = stepAttribute.DependsOn;

        return new Step(name, _job, new MethodAction(methodInfo));
    }
}
#endif