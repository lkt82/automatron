#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class JobVisitor(Stage stage) : MemberVisitor<IEnumerable<Job>>
{
    private readonly Dictionary<string, string[]?> _dependsOnMap = new();

    public IEnumerable<Job> VisitTypes(IEnumerable<Type> types)
    {
        var jobMap = new Dictionary<string, Job>();

        foreach (var type in types)
        {
            foreach (var job in type.Accept(this) ?? Enumerable.Empty<Job>())
            {
                jobMap.Add(job.Name, job);

                yield return job;
            }
        }

        foreach (var jobItem in _dependsOnMap)
        {
            var job = jobMap[jobItem.Key];

            if (jobItem.Value == null)
            {
                continue;
            }
            foreach (var dependsOn in jobItem.Value)
            {
                job.DependsOn.Add(jobMap[dependsOn]);
            }
        }
    }

    public override IEnumerable<Job>? VisitType(Type type)
    {
        var jobAttribute = type.GetAllCustomAttributes<JobAttribute>().ToArray();

        if (jobAttribute.Any())
        {
            yield return CreateJob(type, Merge(jobAttribute));
        }
    }

    private Job CreateJob(Type type, JobAttribute jobAttribute)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name : type.Name;

        var job = new Job(name, stage, p => type.Accept(new StepVisitor(p)) ?? Enumerable.Empty<Step>(), type);

        job.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? Enumerable.Empty<Variable>());

        foreach (var o in type.Accept(new TemplateValueVisitor()) ?? new Dictionary<string, object>())
        {
            job.TemplateValues.Add(o);
        }

        job.TemplateParameters.UnionWith(type.Accept(new TemplateParameterVisitor()) ?? Enumerable.Empty<TemplateParameter>());

        foreach (var jobTemplateParameter in job.TemplateParameters)
        {
            if (job.Stage.TemplateValues.TryGetValue(jobTemplateParameter.Name, out var stageValue))
            {
                jobTemplateParameter.Value = stageValue;
            }
            else if (job.TemplateValues.TryGetValue(jobTemplateParameter.Name, out var jobValue))
            {
                jobTemplateParameter.Value = jobValue;
            } 
  
        }

        _dependsOnMap[name] = jobAttribute.DependsOn;

        return job;
    }

    private static JobAttribute Merge(IEnumerable<JobAttribute> jobAttributes)
    {
        var mergedJobAttribute = new JobAttribute();

        foreach (var jobAttribute in jobAttributes)
        {
            mergedJobAttribute.Name = jobAttribute.Name ?? mergedJobAttribute.Name;
            mergedJobAttribute.DisplayName = jobAttribute.DisplayName ?? mergedJobAttribute.DisplayName;
            mergedJobAttribute.DependsOn = jobAttribute.DependsOn ?? mergedJobAttribute.DependsOn;
            mergedJobAttribute.Condition = jobAttribute.Condition ?? mergedJobAttribute.Condition;
            mergedJobAttribute.Emoji = jobAttribute.Emoji ?? mergedJobAttribute.Emoji;
        }

        return mergedJobAttribute;
    }
}
#endif