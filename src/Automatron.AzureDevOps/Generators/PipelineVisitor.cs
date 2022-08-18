using System;
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PipelineVisitor : SymbolVisitor
{
    private readonly string _vscRoot;
    private readonly string _projectDirectory;
    private readonly ConcreteTypeCollector _concreteClassCollector = new ();

    public PipelineVisitor(string vscRoot,string projectDirectory)
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

        var pipeline = CreatePipeline(symbol, pipelineAttribute, _projectDirectory);

        pipeline.CiTrigger = symbol.Accept(new CiTriggerVisitor());

        pipeline.Schedules = symbol.Accept(new ScheduledTriggerVisitor());

        pipeline.Parameters = symbol.Accept(new ParameterVisitor());

        pipeline.Variables = symbol.Accept(new VariableVisitor());

        pipeline.Pool = symbol.Accept(new PoolVisitor());

        pipeline.Stages = symbol.Accept(new StageVisitor(pipeline));

        Pipelines.Add(pipeline);
    }

    private Pipeline CreatePipeline(ISymbol symbol,PipelineAttribute pipelineAttribute, string projectDirectory)
    {
        var name = !string.IsNullOrEmpty(pipelineAttribute.Name) ? pipelineAttribute.Name : symbol.Name;

        var yamlName = !string.IsNullOrEmpty(pipelineAttribute.YmlName) ? pipelineAttribute.YmlName : name;

        #pragma warning disable CS8604
        var pipeline = new Pipeline(name, yamlName + ".yml", pipelineAttribute.YmlPath, pipelineAttribute.RootPath ?? _vscRoot, projectDirectory);
        #pragma warning restore CS8604

        return pipeline;
    }

    private static void CreateVariables(AttributeData[] attributes, Pipeline pipeline)
    {
        //var variableGroupAttributes = attributes.GetCustomAttributes<VariableGroupAttribute>();
        //var variableAttributes = attributes.GetCustomAttributes<VariableAttribute>();

        //foreach (var variableGroupAttribute in variableGroupAttributes.Where(c =>
        //             c.Pipeline == pipeline.Name || c.Pipeline == null))
        //{
        //    pipeline.Variables.Add(new VariableGroup(variableGroupAttribute.Name));
        //}

        //foreach (var variableAttribute in variableAttributes.Where(c =>
        //             c.Pipeline == pipeline.Name || c.Pipeline == null))
        //{
        //    pipeline.Variables.Add(new Variable(variableAttribute.Name, variableAttribute.Value));
        //}
    }
}