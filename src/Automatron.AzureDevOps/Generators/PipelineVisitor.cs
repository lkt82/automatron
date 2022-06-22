using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PipelineVisitor : SymbolVisitor
{
    private class ConcreteClassCollector : SymbolVisitor
    {
        public List<INamedTypeSymbol> NamedTypes { get; } = new();

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var childSymbol in symbol.GetMembers())
            {
                childSymbol.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.IsStatic)
            {
                return;
            }

            if (symbol.TypeKind == TypeKind.Interface)
            {
                foreach (var nestedTypes in symbol.GetTypeMembers())
                {
                    nestedTypes.Accept(this);
                }

                return;
            }

            if (symbol.TypeKind != TypeKind.Class || symbol.IsAbstract)
            {
                return;
            }

            NamedTypes.Add(symbol);
        }
    }

    private readonly string _vscRoot;
    private readonly string _projectDirectory;
    private readonly ConcreteClassCollector _concreteClassCollector = new ();

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
        var attributes = symbol.GetAllAttributes().ToArray();

        var pipelineAttributes = attributes.GetCustomAttributes<PipelineAttribute>().ToImmutableArray();

        if (!pipelineAttributes.Any())
        {
            return;
        }

        foreach (var pipelineAttribute in pipelineAttributes)
        { 
            var pipeline = CreatePipeline(pipelineAttribute, _projectDirectory);

            var parameterVisitor = new ParameterVisitor(pipeline);

            symbol.Accept(parameterVisitor);

            CreateCiTrigger(attributes, pipeline);

            CreateVariables(attributes, pipeline);

            var secretVariableVisitor = new SecretVariableVisitor(pipeline);

            symbol.Accept(secretVariableVisitor);

            CreateScheduledTriggers(attributes, pipeline);

            CreatePool(attributes, pipeline);

            var stageVisitor = new StageVisitor(pipeline);

            symbol.Accept(stageVisitor);

            Pipelines.Add(pipeline);
        }
    }

    private Pipeline CreatePipeline(PipelineAttribute pipelineAttribute, string projectDirectory)
    {
        var pipeline = new Pipeline(pipelineAttribute.Name, pipelineAttribute.YmlName, pipelineAttribute.YmlPath, pipelineAttribute.RootPath ?? _vscRoot, projectDirectory);

        return pipeline;
    }

    private static void CreateCiTrigger(AttributeData[] attributes, Pipeline pipeline)
    {
        var ciTriggerAttribute = attributes.GetCustomAttributes<CiTriggerAttribute>().FirstOrDefault(c => c.Pipeline == pipeline.Name || c.Pipeline == null);

        if (ciTriggerAttribute != null)
        {
            CreateCiTrigger(pipeline, ciTriggerAttribute);
        }
    }

    private static void CreateCiTrigger(Pipeline pipeline, CiTriggerAttribute ciTriggerAttribute)
    {
        if (ciTriggerAttribute.Disabled)
        {
            pipeline.CiTrigger = new DisabledCiTrigger();
            return;
        }

        var trigger = new CiTrigger
        {
            Batch = ciTriggerAttribute.Batch
        };

        if (ciTriggerAttribute.IncludeBranches != null || ciTriggerAttribute.ExcludeBranches != null)
        {
            trigger.Branches = new TriggerBranches
            {
                Include = ciTriggerAttribute.IncludeBranches,
                Exclude = ciTriggerAttribute.ExcludeBranches
            };
        }

        if (ciTriggerAttribute.IncludePaths != null || ciTriggerAttribute.ExcludePaths != null)
        {
            trigger.Paths = new TriggerPaths
            {
                Include = ciTriggerAttribute.IncludePaths,
                Exclude = ciTriggerAttribute.ExcludePaths
            };
        }
        pipeline.CiTrigger = trigger;
    }

    private static void CreateVariables(AttributeData[] attributes, Pipeline pipeline)
    {
        var variableGroupAttributes = attributes.GetCustomAttributes<VariableGroupAttribute>();
        var variableAttributes = attributes.GetCustomAttributes<VariableAttribute>();

        foreach (var variableGroupAttribute in variableGroupAttributes.Where(c =>
                     c.Pipeline == pipeline.Name || c.Pipeline == null))
        {
            pipeline.Variables.Add(new VariableGroup(variableGroupAttribute.Name));
        }

        foreach (var variableAttribute in variableAttributes.Where(c =>
                     c.Pipeline == pipeline.Name || c.Pipeline == null))
        {
            pipeline.Variables.Add(new Variable(variableAttribute.Name, variableAttribute.Value));
        }
    }

    private static void CreateScheduledTriggers(AttributeData[] attributes, Pipeline pipeline)
    {
        var scheduledTriggerAttributes = attributes.GetCustomAttributes<ScheduledTriggerAttribute>().Where(c => c.Pipeline == pipeline.Name || c.Pipeline == null);

        foreach (var scheduledTriggerAttribute in scheduledTriggerAttributes)
        {
            CreateScheduledTrigger(pipeline, scheduledTriggerAttribute);
        }
    }

    private static void CreateScheduledTrigger(Pipeline pipeline, ScheduledTriggerAttribute scheduledTriggerAttribute)
    {
        var scheduledTrigger = new ScheduledTrigger(scheduledTriggerAttribute.Cron)
        {
            Always = scheduledTriggerAttribute.Always,
            DisplayName = scheduledTriggerAttribute.DisplayName
        };

        if (scheduledTriggerAttribute.IncludeBranches != null || scheduledTriggerAttribute.ExcludeBranches != null)
        {
            scheduledTrigger.Branches = new TriggerBranches
            {
                Include = scheduledTriggerAttribute.IncludeBranches,
                Exclude = scheduledTriggerAttribute.ExcludeBranches
            };
        }
        pipeline.Schedules.Add(scheduledTrigger);
    }

    private static void CreatePool(AttributeData[] attributes, Pipeline pipeline)
    {
        var poolAttribute = attributes.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == pipeline.Name || c.Target == null);

        if (poolAttribute != null)
        {
            pipeline.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
        }
    }
}