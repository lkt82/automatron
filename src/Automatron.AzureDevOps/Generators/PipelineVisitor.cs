using System;
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PipelineVisitor : SymbolVisitor
{
    private readonly string _vscRoot;
    private readonly string _projectDirectory;
    private readonly string _command;
    private readonly ConcreteTypeCollector _concreteClassCollector = new ();

    public PipelineVisitor(string vscRoot,string projectDirectory,string command)
    {
        if (string.IsNullOrEmpty(vscRoot))
        {
            throw new ArgumentException("vscRoot was empty",nameof(vscRoot));
        }

        if (string.IsNullOrEmpty(projectDirectory))
        {
            throw new ArgumentException("projectDirectory was empty", nameof(projectDirectory));
        }

        _vscRoot = vscRoot;
        _projectDirectory = projectDirectory;
        _command = command;
    }

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        symbol.Accept(_concreteClassCollector);

        foreach (var concreteClass in _concreteClassCollector.NamedTypes)
        {
            concreteClass.Accept(this);
        }
    }

    public List<Pipeline> Pipelines { get; } = new();

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        VisitPipelineType(symbol);
    }

    private void VisitPipelineType(INamedTypeSymbol symbol)
    {
        var pipelineAttribute = symbol.GetAllAttributes().GetCustomAttribute<PipelineAttribute>();

        if (pipelineAttribute == null)
        {
            return;
        }

        var pipeline = CreatePipeline(symbol, pipelineAttribute);

        pipeline.CiTrigger = symbol.Accept(new CiTriggerVisitor());

        pipeline.Schedules = symbol.Accept(new ScheduledTriggerVisitor());

        pipeline.Parameters = symbol.Accept(new ParameterVisitor());

        pipeline.Variables = symbol.Accept(new VariableVisitor());

        pipeline.Pool = symbol.Accept(new PoolVisitor());

        pipeline.Stages = symbol.Accept(new StageVisitor(pipeline));

        Pipelines.Add(pipeline);
    }

    private Pipeline CreatePipeline(ISymbol symbol,PipelineAttribute pipelineAttribute)
    {
        var name = !string.IsNullOrEmpty(pipelineAttribute.Name) ? pipelineAttribute.Name : symbol.Name;

        var yamlName = !string.IsNullOrEmpty(pipelineAttribute.YmlName) ? pipelineAttribute.YmlName : name;

        #pragma warning disable CS8604
        var pipeline = new Pipeline(name, yamlName + ".yml", pipelineAttribute.YmlDir, pipelineAttribute.RootDir ?? _vscRoot, _projectDirectory, _command, symbol);
#pragma warning restore CS8604

        return pipeline;
    }
}