﻿#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class JobVisitor : MemberVisitor<IEnumerable<Job>>
{
    private readonly Stage _stage;

    private readonly Dictionary<string, string[]?> _dependsOnMap = new();

    public JobVisitor(Stage stage)
    {
        _stage = stage;
    }

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
        var jobAttribute = type.GetAllCustomAttribute<JobAttribute>();

        if (jobAttribute != null)
        {
            yield return CreateJob(type, jobAttribute);
        }
    }

    private Job CreateJob(Type type, JobAttribute jobAttribute)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name : type.Name;

        _dependsOnMap[name] = jobAttribute.DependsOn;

        var job = new Job(name, _stage, p => type.Accept(new StepVisitor(p)) ?? Enumerable.Empty<Step>(), type);
        job.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? Enumerable.Empty<Variable>());

        return job;
    }
}
#endif