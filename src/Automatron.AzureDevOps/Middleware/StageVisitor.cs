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
        var stageAttributes = type.GetAllCustomAttributes<StageAttribute>().ToArray();

        if (stageAttributes.Any())
        {
            yield return CreateStage(type, Merge(stageAttributes));
        }
    }

    private Stage CreateStage(Type type, StageAttribute stageAttribute)
    {
        var name = !string.IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name : type.Name;

        var stage = new Stage(name, _pipeline, s =>
        {
            s.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? Enumerable.Empty<Variable>());
            foreach (var o in type.Accept(new TemplateValueVisitor()) ?? new Dictionary<string, object>())
            {
                s.TemplateValues.Add(o);
            }

            return new JobVisitor(s).VisitTypes(type.GetAllNestedTypes().Append(type));
        }, type);

        _dependsOnMap[name] = stageAttribute.DependsOn;

        return stage;
    }

    private static StageAttribute Merge(IEnumerable<StageAttribute> stageAttributes)
    {
        var mergedStageAttribute = new StageAttribute();

        foreach (var stageAttribute in stageAttributes)
        {
            mergedStageAttribute.Name = stageAttribute.Name ?? mergedStageAttribute.Name;
            mergedStageAttribute.DisplayName = stageAttribute.DisplayName ?? mergedStageAttribute.DisplayName;
            mergedStageAttribute.DependsOn = stageAttribute.DependsOn ?? mergedStageAttribute.DependsOn;
            mergedStageAttribute.Condition = stageAttribute.Condition ?? mergedStageAttribute.Condition;
            mergedStageAttribute.Emoji = stageAttribute.Emoji ?? mergedStageAttribute.Emoji;
        }

        return mergedStageAttribute;
    }
}
#endif