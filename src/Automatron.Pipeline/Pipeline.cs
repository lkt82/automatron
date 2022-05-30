﻿using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.Pipeline
{
    [Pipeline("../../",
        YmlPath = "../../"
    )]
    [CiTrigger(
        Batch = true,
        IncludeBranches = new[] { "main" },
        IncludePaths = new[] { "src" }
    )]
    [Pool(VmImage = "ubuntu-latest")]
    public class Pipeline
    {
        private static int Main(string[] args) => new TaskRunner<Pipeline>().Run(args);

        [Stage]
        [Job]
        public void Ci() { }

        [AutomatronTask(nameof(Ci),DisplayName =nameof(Build))]
        [DependentFor(nameof(Ci))]
        public void Build()
        {

        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack), SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Build))]
        public void Pack()
        {

        }

    }
}
