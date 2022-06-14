﻿using System;

namespace Automatron.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ParameterAttribute : Attribute
{
    public string Description { get; }

    public ParameterAttribute(string description)
    {
        Description = description;
    }
}