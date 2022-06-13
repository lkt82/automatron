﻿using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StageVisitor : SymbolVisitor
{
    private readonly Pipeline _pipeline;

    public List<Stage> Stages { get; } = new();

    public StageVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var publicMembers = symbol.GetAllPublicMethods();

        foreach (var member in publicMembers)
        {
            member.Accept(this);
        }

        foreach (var stage in Stages)
        {
            var jobVisitor = new JobVisitor(stage);
            symbol.Accept(jobVisitor);
            stage.Jobs.AddRange(jobVisitor.Jobs);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        foreach (var stageAttribute in symbol.GetCustomAttributes<StageAttribute>())
        {
            if (stageAttribute.Pipeline != _pipeline.Name && stageAttribute.Pipeline != null)
            {
                continue;
            }

            var stage = CreateStage(stageAttribute, symbol);

            Stages.Add(stage);
        }
    }

    private Stage CreateStage(StageAttribute stageAttribute, ISymbol member)
    {
        var name = !string.IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name! : member.Name;

        var stage = new Stage(_pipeline, name, stageAttribute.DisplayName, stageAttribute.DependsOn, stageAttribute.Condition);

        var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

        if (poolAttribute != null)
        {
            stage.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
        }

        if (stageAttribute.TemplateSymbol != null)
        {
            stage.Template = stageAttribute.TemplateSymbol.Name;
        }

        return stage;
    }
}