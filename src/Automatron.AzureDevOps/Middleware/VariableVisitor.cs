#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class VariableVisitor : MemberVisitor<IEnumerable<Variable>>
{
    public override IEnumerable<Variable>? VisitType(Type type)
    {
        foreach (var propertyInfo in type.GetAllProperties())
        {
            foreach (var variableBinding in propertyInfo.Accept(this) ?? Enumerable.Empty<Variable>())
            {
                yield return variableBinding;
            }
        }
    }

    public override IEnumerable<Variable>? VisitProperty(PropertyInfo propertyInfo)
    {
        var variableAttribute = propertyInfo.GetAllCustomAttribute<VariableAttribute>();

        if (variableAttribute != null)
        {
            yield return CreateVariable(propertyInfo, variableAttribute);
        }
    }

    private static Variable CreateVariable(PropertyInfo propertyInfo, VariableAttribute variableAttribute)
    {
        var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : propertyInfo.Name;

        return new Variable(name, variableAttribute.Description, propertyInfo);
    }
}
#endif