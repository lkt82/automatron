using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class TemplateJobVisitor : JobVisitor
{
    public TemplateJobVisitor(Stage stage) : base(stage)
    {
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        foreach (var attribute in symbol.GetCustomAbstractAttributes<JobAttribute>())
        {
            if (!string.IsNullOrEmpty(attribute.Stage))
            {
                continue;
            }

            if (attribute is DeploymentJobAttribute deploymentJobAttribute)
            {
                CreateDeploymentJob(deploymentJobAttribute, symbol);
            }
            else
            {
                CreateJob(attribute, symbol);
            }
        }
    }
}