using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StageVisitor : SymbolVisitor, IComparer<Stage>
{
    private readonly Pipeline _pipeline;

    public List<Stage> Stages { get; } = new();

    private readonly Dictionary<Stage, INamedTypeSymbol> _templates = new();

    public StageVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var methods = symbol.GetAllPublicMethods();

        foreach (var method in methods)
        {
            method.Accept(this);
        }

        foreach (var stage in Stages)
        {
            JobVisitor jobVisitor;

             if (_templates.ContainsKey(stage))
            {
                 jobVisitor = new TemplateJobVisitor(stage);
                 _templates[stage].Accept(jobVisitor);
            }
            else
            {
                jobVisitor = new JobVisitor(stage);
                symbol.Accept(jobVisitor);
            }
        }

        _pipeline.Stages.AddRange(Stages);
        _pipeline.Stages.Sort(this);
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        //foreach (var stageAttribute in symbol.GetCustomAttributes<StageAttribute>())
        //{
        //    if (stageAttribute.Pipeline != _pipeline.Name && stageAttribute.Pipeline != null)
        //    {
        //        continue;
        //    }

        //    var stage = CreateStage(stageAttribute, symbol);

        //    Stages.Add(stage);
        //}
    }

    //private Stage CreateStage(StageAttribute stageAttribute, ISymbol member)
    //{
    //    var name = !string.IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name! : member.Name;

    //    var stage = new Stage(_pipeline, name, stageAttribute.DisplayName, stageAttribute.DependsOn, stageAttribute.Condition);

    //    var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

    //    if (poolAttribute != null)
    //    {
    //        stage.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
    //    }

    //    if (stageAttribute.TemplateSymbol != null)
    //    {
    //        stage.TemplateName = stageAttribute.TemplateSymbol.Name;
    //        _templates[stage] = stageAttribute.TemplateSymbol;
    //    }

    //    return stage;
    //}

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