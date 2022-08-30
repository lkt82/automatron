﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StepVisitor : SymbolVisitor<IEnumerable<Step>>, IComparer<Step>
{
    private readonly IJob _job;

    private Dictionary<string,Step> Steps { get; } = new();

    private IEnumerable<string>? VariableReferences { get; set; }

    private IDictionary<string, object>? EnvVariable { get; set; }


    public StepVisitor(IJob job)
    {
        _job = job;
    }

    public override IEnumerable<Step> VisitNamedType(INamedTypeSymbol symbol)
    {
        var steps = VisitStepType(symbol);

        var list = new List<Step>(steps);
        list.Sort(this);

        return list;
    }

    public override IEnumerable<Step> VisitMethod(IMethodSymbol symbol)
    {
        return Steps.ContainsKey(symbol.Name) ? Enumerable.Empty<Step>() : VisitStepType(symbol);
    }

    private IEnumerable<Step> VisitStepType(INamedTypeSymbol symbol)
    {
        //VariableReferences = symbol.Accept(new VariableReferenceVisitor());
        EnvVariable = symbol.Accept(new EnvVariableVisitor(_job));

        var methods = symbol.GetAllMethods();

        foreach (var method in methods)
        {
            var steps = method.Accept(this);

            if (steps == null)
            {
                continue;
            }

            foreach (var step in steps)
            {
                yield return step;
            }
        }

        foreach (var type in symbol.GetAllTypeMembers())
        {
            foreach (var step in VisitStepType(type))
            {
                yield return step;
            }
        }
    }

    private IEnumerable<Step> VisitStepType(IMethodSymbol symbol)
    {
        foreach (var nodeAttribute in symbol.GetCustomAbstractAttributes<NodeAttribute>())
        {
            var step = CreateStep(nodeAttribute, symbol);

            Steps[symbol.Name] = step;

            yield return step;
        }
    }

    private Step CreateStep(NodeAttribute nodeAttribute, IMethodSymbol member)
    {
        return nodeAttribute switch
        {
            CheckoutAttribute checkoutAttribute => CreateCheckoutTask(member, checkoutAttribute),
            StepAttribute stepAttribute => CreateAutomatronScript(member, stepAttribute),
            _ => throw new NotSupportedException()
        };
    }

    private Step CreateAutomatronScript(IMethodSymbol member, StepAttribute stepAttribute)
    {
        var stepName = string.IsNullOrEmpty(stepAttribute.Name) ? member.Name : stepAttribute.Name;

        #pragma warning disable CS8604
        var taskName = GetTaskName(stepName);
        #pragma warning restore CS8604

        var displayName = string.IsNullOrEmpty(stepAttribute.Emoji) ? stepAttribute.DisplayName : $"{stepAttribute.Emoji} {stepName}";

        var dependsOn = stepAttribute.DependsOn == null ? Array.Empty<string>() : stepAttribute.DependsOn.Select(c => Steps[c].Name!).ToArray();

        var skip = dependsOn.Select(GetTaskName).ToArray();

        return new AutomatronScript(_job, new[] { taskName }, skip, false, true, Array.Empty<string>())
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = stepAttribute.Condition,
            DependsOn = dependsOn,
            WorkingDirectory = stepAttribute.WorkingDirectory ?? GetWorkingDirectory(),
            Env = EnvVariable //VariableReferences?.ToDictionary(GetEnvVarName, c => (object)$"$({c})")
        };
    }

    private Step CreateCheckoutTask(IMethodSymbol member, CheckoutAttribute checkoutAttribute)
    {
        var stepName = checkoutAttribute.Name;
        var displayName = string.IsNullOrEmpty(checkoutAttribute.Emoji) ? checkoutAttribute.DisplayName : $"{checkoutAttribute.Emoji} {stepName}";

        return new CheckoutTask(_job, checkoutAttribute.Source)
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = checkoutAttribute.Condition
        };
    }

    private string GetTaskName(string stepName)
    {
       var tokens = new List<string> { _job.Stage.Pipeline.Name };

       if (_job.Stage.Path != _job.Stage.Pipeline.Path)
       {
           tokens.Add(_job.Stage.Name);
       }

       if (_job.Path != _job.Stage.Path)
       {
           tokens.Add(_job.Name);
       }

       tokens.Add(stepName);
        
       return string.Join("-", tokens);
    }

    private string GetWorkingDirectory()
    {
        var fullRoot = PathExtensions.GetUnixPath(Path.GetFullPath(_job.Stage.Pipeline.RootPath)) + "/";

        var path = PathExtensions.GetUnixPath(PathExtensions.GetRelativePath(fullRoot, _job.Stage.Pipeline.ProjectDirectory));

        return path;
    }

    private static string GetEnvVarName(string name)
    {
        var envVarName = new StringBuilder();

        for (var index = 0; index < name.Length; index++)
        {
            var n = name[index];
            if (index > 0 && char.IsLower(name[index - 1]) && char.IsUpper(n))
            {
                envVarName.Append('_');
                envVarName.Append(n);
            }
            else if (char.IsLower(n))
            {
                envVarName.Append(char.ToUpper(n));
            }
            else
            {
                envVarName.Append(n);
            }
        }

        return envVarName.ToString();
    }

    public int Compare(Step? x, Step? y)
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