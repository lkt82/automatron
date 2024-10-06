#if NETSTANDARD2_0
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Automatron.Collections;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StageVisitor : SymbolVisitor<IEnumerable<Stage>>
{
    private readonly Pipeline _pipeline;

    private Dictionary<string,Stage> Stages { get; } = new();

    public StageVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override IEnumerable<Stage> VisitNamedType(INamedTypeSymbol symbol)
    {
        VisitStageType(symbol);

        foreach (var stage in Stages.Values)
        {
            stage.Jobs = stage.Symbol.Accept(new JobVisitor(stage));
        }

        var list = Stages.Values.TopologicalSort(x => x.DependsOn ?? Enumerable.Empty<string>(), x => x.Name);

        return list;
    }

    private void VisitStageType(INamedTypeSymbol symbol)
    {
        if (Stages.ContainsKey(symbol.Name))
        {
            return;
        }

        var stageAttributes = symbol.GetAllCustomAttributes<StageAttribute>().ToArray();

        if (stageAttributes.Any())
        {
            var stage = CreateStage(Merge(stageAttributes), symbol);

            Stages[symbol.Name] = stage;
        }

        foreach (var type in symbol.GetAllTypeMembers())
        {
            VisitStageType(type);
        }
    }

    private Stage CreateStage(StageAttribute stageAttribute, ISymbol symbol)
    {
        var name = !string.IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name! : symbol.Name;

        var stage = new Stage(_pipeline, name, stageAttribute.DisplayName, stageAttribute.DependsOn, stageAttribute.Condition, symbol)
            {
                Pool = symbol.Accept(new PoolVisitor()),
                Variables = symbol.Accept(new VariableVisitor()),
                TemplateParameters = symbol.Accept(new TemplateValueVisitor())
            };

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