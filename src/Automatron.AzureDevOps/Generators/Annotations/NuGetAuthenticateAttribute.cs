﻿using System;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NuGetAuthenticateAttribute : NodeAttribute
    {
        //public override Step Create(ISymbol symbol, IJob job)
        //{
        //    return new NuGetAuthenticateTask(job)
        //    {
        //        Name = Name,
        //        DisplayName = DisplayName
        //    };
        //}
    }
}
