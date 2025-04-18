﻿using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class PipelineTriggerAttribute(string name, string source) : Attribute
{
    public string Name { get; } = name;

    public string Source { get; set; } = source;

    public string? Project { get; set; }

    public string[]? Stages { get; set; }

    public string[]? Tags { get; set; }

    public string[]? IncludeBranches { get; set; }

    public string[]? ExcludeBranches { get; set; }
}