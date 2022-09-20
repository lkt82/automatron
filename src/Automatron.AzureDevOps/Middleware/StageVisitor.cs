#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class StageVisitor : MemberVisitor<IEnumerable<Stage>>
{
    private readonly Pipeline _pipeline;

    private readonly Dictionary<string, string[]?> _dependsOnMap = new();

    public StageVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public IEnumerable<Stage> VisitTypes(IEnumerable<Type> types)
    {
        var stageMap = new Dictionary<string, Stage>();

        foreach (var type in types)
        {
            foreach (var stage in type.Accept(this) ?? Enumerable.Empty<Stage>())
            {
                stageMap.Add(stage.Name, stage);

                yield return stage;
            }
        }

        foreach (var stageItem in _dependsOnMap)
        {
            var stage = stageMap[stageItem.Key];

            if (stageItem.Value == null)
            {
                continue;
            }
            foreach (var dependsOn in stageItem.Value)
            {
                stage.DependsOn.Add(stageMap[dependsOn]);
            }
        }
    }

    public override IEnumerable<Stage>? VisitType(Type type)
    {
        var stageAttribute = type.GetAllCustomAttribute<StageAttribute>();

        if (stageAttribute != null)
        {
            yield return CreateStage(type, stageAttribute);
        }
    }

    private Stage CreateStage(Type type, StageAttribute stageAttribute)
    {
        var name = !string.IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name : type.Name;

        _dependsOnMap[name] = stageAttribute.DependsOn;

        var stage = new Stage(name, _pipeline, p => new JobVisitor(p).VisitTypes(type.GetAllNestedTypes().Append(type)), type);

        stage.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? Enumerable.Empty<Variable>());

        return stage;
    }
}
#endif