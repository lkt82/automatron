#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class ParameterVisitor : MemberVisitor<IEnumerable<Parameter>>
{
    public override IEnumerable<Parameter>? VisitType(Type type)
    {
        foreach (var propertyInfo in type.GetAllProperties())
        {
            foreach (var parameter in propertyInfo.Accept(this) ?? Enumerable.Empty<Parameter>())
            {
                yield return parameter;
            }
        }
    }

    public override IEnumerable<Parameter>? VisitProperty(PropertyInfo propertyInfo)
    {
        var variableAttribute = propertyInfo.GetAllCustomAttribute<ParameterAttribute>();

        if (variableAttribute != null)
        {
            yield return CreateParameter(propertyInfo, variableAttribute);
        }
    }

    private static Parameter CreateParameter(PropertyInfo propertyInfo, ParameterAttribute variableAttribute)
    {
        var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : propertyInfo.Name;

        return new Parameter(name, variableAttribute.DisplayName, propertyInfo);
    }
}
#endif