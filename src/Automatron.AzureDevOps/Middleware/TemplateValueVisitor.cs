#if NET6_0
using System;
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class TemplateValueVisitor : MemberVisitor<IDictionary<string, object>>
{
    public override IDictionary<string, object>? VisitType(Type type)
    {
        var parameters = new Dictionary<string, object>();

        var parameterAttributes = type.GetAllCustomAttributes<TemplateParameterAttribute>();
        foreach (var variableAttribute in parameterAttributes)
        {
            if (variableAttribute.Name == null || variableAttribute.Value == null)
            {
                continue;
            }
            parameters.Add(variableAttribute.Name, variableAttribute.Value);
        }

        var deploymentJobAttributes = type.GetAllCustomAttributes<DeploymentJobAttribute>();
        foreach (var deploymentJobAttribute in deploymentJobAttributes)
        {
            if (string.IsNullOrEmpty(deploymentJobAttribute.Environment) || deploymentJobAttribute.Environment.StartsWith("$"))
            {
                continue;
            }
            parameters[nameof(deploymentJobAttribute.Environment)] = deploymentJobAttribute.Environment;
        }

        return parameters;
    }
}

#endif