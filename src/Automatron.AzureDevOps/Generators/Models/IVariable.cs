﻿#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public interface IVariable
{
    string Name { get; set; }
}
#endif