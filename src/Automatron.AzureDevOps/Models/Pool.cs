﻿namespace Automatron.AzureDevOps.Models;

public sealed class Pool
{
    public Pool(string? name, string? vmImage)
    {
        Name = name;
        VmImage = vmImage;
    }

    public string? Name { get; set; }

    public string? VmImage { get; set; }
}