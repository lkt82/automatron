﻿namespace Automatron.AzureDevOps.Generators.Models
{
    public class Pool
    {
        public Pool(string? name, string? vmImage)
        {
            Name = name;
            VmImage = vmImage;
        }

        public string? Name { get; set; }

        public string? VmImage { get; set; }
    }
}
