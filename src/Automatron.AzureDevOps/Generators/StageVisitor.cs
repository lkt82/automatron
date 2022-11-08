using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StageVisitor : SymbolVisitor<IEnumerable<Stage>>, IComparer<Stage>
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

        var list = new List<Stage>(Stages.Values);
        list.Sort(this);

        return list;
    }

    private void VisitStageType(INamedTypeSymbol symbol)
    {
        if (Stages.ContainsKey(symbol.Name))
        {
            return;
        }

        var stageAttribute = symbol.GetAllAttributes().GetCustomAttribute<StageAttribute>();

        if (stageAttribute != null)
        {
            var stage = CreateStage(stageAttribute, symbol);

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
                TemplateParameters = symbol.Accept(new TemplateParameterVisitor())
            };

        return stage;
    }

    public int Compare(Stage? x, Stage? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        if (x.DependsOn != null && x.DependsOn.Contains(y.Name))
        {
            return 1;
        }

        if (y.DependsOn != null && y.DependsOn.Contains(x.Name))
        {
            return -1;
        }

        if (y.DependsOn != null && x.DependsOn == null)
        {
            return -1;
        }

        return 0;
    }
}