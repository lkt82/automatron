#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.AzureDevOps.IO;
using Automatron.CodeAnalysis;
using Automatron.Collections;
using Microsoft.CodeAnalysis;
using static Automatron.AzureDevOps.Generators.Models.KubeLoginInstallerTask;
using static Automatron.AzureDevOps.Generators.Models.PulumiTask;

namespace Automatron.AzureDevOps.Generators;

internal class StepVisitor : SymbolVisitor<IEnumerable<Step>>
{
    private readonly IJob _job;

    private readonly Dictionary<string, string[]?> _dependsOnMap = new();

    private Dictionary<string, object>? EnvVariable { get; set; }


    public StepVisitor(IJob job)
    {
        _job = job;
    }

    public override IEnumerable<Step> VisitNamedType(INamedTypeSymbol symbol)
    {
        EnvVariable = symbol.Accept(new EnvVariableVisitor());

        var stepMap = new Dictionary<string, Step>();

        foreach (var method in symbol.GetAllMethods())
        {
            var steps = method.Accept(this);

            if (steps == null)
            {
                continue;
            }

            foreach (var step in steps)
            {
                stepMap.Add(step.Id, step);
            }
        }

        CalculateDependencies(stepMap);

        var sortedList = stepMap.Values.TopologicalSort(x => x.DependsOn);

        return sortedList;
    }

    private void CalculateDependencies(Dictionary<string, Step> stepMap)
    {
        foreach (var stepName in _dependsOnMap)
        {
            var step = stepMap[stepName.Key];

            if (stepName.Value == null)
            {
                continue;
            }

            foreach (var dependsOn in stepName.Value)
            {
                if (stepMap.ContainsKey(dependsOn))
                {
                    step.DependsOn.Add(stepMap[dependsOn]);
                }
            }
        }

        var list = stepMap.Values.ToList();

        for (int i = 0; i < list.Count; i++)
        {
            var current = list[i];

            if (current.Name == null)
            {
                if (i + 1 < list.Count)
                {
                    var next = list[i + 1];
                    current.DependsOn.UnionWith(next.DependsOn);
                    next.DependsOn.Add(current);
                }
            }
        }
    }

    public override IEnumerable<Step> VisitMethod(IMethodSymbol symbol)
    {
        var checkoutAttribute = symbol.GetAllCustomAttributes<CheckoutAttribute>().ToArray();

        if (checkoutAttribute.Any())
        {
            yield return CreateCheckoutTask(symbol, checkoutAttribute.Last());
        }

        var nuGetAuthenticateAttribute = symbol.GetAllCustomAttributes<NuGetAuthenticateAttribute>().ToArray();

        if (nuGetAuthenticateAttribute.Any())
        {
            yield return CreateNuGetAuthenticateTask(symbol, nuGetAuthenticateAttribute.Last());
        }

        var pulumiAttribute = symbol.GetAllCustomAttributes<PulumiAttribute>().ToArray();

        if (pulumiAttribute.Any())
        {
            yield return CreatePulumiTask(symbol, pulumiAttribute.Last());
        }

        var kubeLoginInstallerAttribute = symbol.GetAllCustomAttributes<KubeLoginInstallerAttribute>().ToArray();

        if (kubeLoginInstallerAttribute.Any())
        {
            yield return CreateKubeLoginInstallerTask(symbol, kubeLoginInstallerAttribute.Last());
        }

        var stepAttributes = symbol.GetAllCustomAttributes<StepAttribute>().ToArray();

        if (stepAttributes.Any())
        {
            yield return CreateAutomatronScript(symbol, Merge(stepAttributes));
        }
    }

    private Step CreateKubeLoginInstallerTask(IMethodSymbol member, KubeLoginInstallerAttribute kubeLoginInstallerAttribute)
    {
        var stepName = kubeLoginInstallerAttribute.Name;

        var displayName = string.IsNullOrEmpty(kubeLoginInstallerAttribute.Emoji) ? kubeLoginInstallerAttribute.DisplayName : $"{kubeLoginInstallerAttribute.Emoji} {stepName}";

        KubeLoginInstallerInputs? inputs = null;

        if (kubeLoginInstallerAttribute.Version != null)
        {
            inputs = new KubeLoginInstallerInputs
            {
                KubeLoginVersion = kubeLoginInstallerAttribute.Version
            };
        }

        return new KubeLoginInstallerTask(_job, inputs)
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = kubeLoginInstallerAttribute.Condition
        };
    }

    private Step CreatePulumiTask(IMethodSymbol member, PulumiAttribute pulumiAttribute)
    {
        var stepName = pulumiAttribute.Name;

        var displayName = string.IsNullOrEmpty(pulumiAttribute.Emoji) ? pulumiAttribute.DisplayName : $"{pulumiAttribute.Emoji} {stepName}";

        PulumiTaskInputs? inputs = null;

        if (pulumiAttribute.Command != null || pulumiAttribute.Stack != null || pulumiAttribute.Cwd != null || pulumiAttribute.Args != null)
        {
            inputs = new PulumiTaskInputs
            {
                Command = pulumiAttribute.Command,
                Stack = pulumiAttribute.Stack,
                Cwd = pulumiAttribute.Cwd,
                Args = pulumiAttribute.Args
            };
        }

        //var last = Steps.Keys.LastOrDefault();

        //var dependsOn = last != null ? new[] { last } : null;

        return new PulumiTask(_job, inputs)
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = pulumiAttribute.Condition,
           /// DependsOn = dependsOn
        };
    }

    private Step CreateNuGetAuthenticateTask(IMethodSymbol member, NuGetAuthenticateAttribute nugetAuthenticateAttribute)
    {
        var stepName = nugetAuthenticateAttribute.Name;
        var displayName = string.IsNullOrEmpty(nugetAuthenticateAttribute.Emoji) ? nugetAuthenticateAttribute.DisplayName : $"{nugetAuthenticateAttribute.Emoji} {stepName}";

        NuGetAuthenticateTask.NuGetAuthenticateInputs? input = null;

        if (!string.IsNullOrEmpty(nugetAuthenticateAttribute.NugetServiceConnections) ||
            nugetAuthenticateAttribute.ReinstallCredentialProvider)
        {
            input = new NuGetAuthenticateTask.NuGetAuthenticateInputs
            {
                NuGetServiceConnections = nugetAuthenticateAttribute.NugetServiceConnections,
                ForceReinstallCredentialProvider = nugetAuthenticateAttribute.ReinstallCredentialProvider == false ? null : nugetAuthenticateAttribute.ReinstallCredentialProvider
            };
        }

       // var last = Steps.Keys.LastOrDefault();

       // var dependsOn = last != null ? new[] { last } : null;

        return new NuGetAuthenticateTask(_job, input)
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = nugetAuthenticateAttribute.Condition,
            //DependsOn = dependsOn
        };
    }

    private Step CreateAutomatronScript(ISymbol member, StepAttribute stepAttribute)
    {
        var stepName = string.IsNullOrEmpty(stepAttribute.Name) ? member.Name : stepAttribute.Name;

        var displayName = string.IsNullOrEmpty(stepAttribute.Emoji) ? stepAttribute.DisplayName : $"{stepAttribute.Emoji} {stepName}";

        _dependsOnMap[stepName ?? throw new InvalidOperationException()] = stepAttribute.DependsOn;

        // ReSharper disable once RedundantSuppressNullableWarningExpression
        return new AutomatronScript(_job, stepName!)
        {
            Name = stepName,
            Id = stepName,
            DisplayName = displayName,
            Condition = stepAttribute.Condition,
            //DependsOn = stepAttribute.DependsOn,
            WorkingDirectory = stepAttribute.WorkingDirectory ?? GetWorkingDirectory(),
            Env = EnvVariable
        };
    }

    private Step CreateCheckoutTask(IMethodSymbol member, CheckoutAttribute checkoutAttribute)
    {
        var stepName = checkoutAttribute.Name;
        var displayName = string.IsNullOrEmpty(checkoutAttribute.Emoji) ? checkoutAttribute.DisplayName : $"{checkoutAttribute.Emoji} {stepName}";

        //var last = Steps.Keys.LastOrDefault();

        //var dependsOn = last != null ? new[] { last } : null;

        return new CheckoutTask(_job, checkoutAttribute.Source)
        {
            Name = stepName,
            DisplayName = displayName,
            Condition = checkoutAttribute.Condition,
            FetchDepth = checkoutAttribute.FetchDepth == 1 ? null : checkoutAttribute.FetchDepth,
            Clean = checkoutAttribute.Clean == false ? null : checkoutAttribute.Clean,
            Lfs = checkoutAttribute.Lfs == false ? null : checkoutAttribute.Lfs,
            Submodules = checkoutAttribute.Submodules == false ? null : checkoutAttribute.Submodules,
            Path = checkoutAttribute.Path,
            PersistCredentials = checkoutAttribute.PersistCredentials == false ? null : checkoutAttribute.PersistCredentials,
            //DependsOn = dependsOn
        };
    }

    private static StepAttribute Merge(IEnumerable<StepAttribute> stepAttributes)
    {
        var mergedStepAttribute = new StepAttribute();

        foreach (var stepAttribute in stepAttributes)
        {
            mergedStepAttribute.Name = stepAttribute.Name ?? mergedStepAttribute.Name;
            mergedStepAttribute.DisplayName = stepAttribute.DisplayName ?? mergedStepAttribute.DisplayName;
            mergedStepAttribute.DependsOn = stepAttribute.DependsOn ?? mergedStepAttribute.DependsOn;
            mergedStepAttribute.Condition = stepAttribute.Condition ?? mergedStepAttribute.Condition;
            mergedStepAttribute.Emoji = stepAttribute.Emoji ?? mergedStepAttribute.Emoji;
        }

        return mergedStepAttribute;
    }

    private string GetWorkingDirectory()
    {
        var fullRoot = PathExtensions.GetUnixPath(Path.GetFullPath(_job.Stage.Pipeline.RootDir)) + "/";

        var path = PathExtensions.GetUnixPath(PathExtensions.GetRelativePath(fullRoot, _job.Stage.Pipeline.ProjectDir));

        return path;
    }
}
#endif