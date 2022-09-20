#if NET6_0
using System;
using System.Collections.Generic;
using System.Reflection;
using Automatron.Reflection;
using Automatron.Tasks.Annotations;
using Automatron.Tasks.Models;

namespace Automatron.Tasks.Middleware;

internal class ParameterVisitor : MemberVisitor<IEnumerable<Parameter>>
{
    public override IEnumerable<Parameter> VisitType(Type type)
    {
        var properties = type.GetAllProperties();

        foreach (var property in properties)
        {
            var parameters = property.Accept(this) ?? Array.Empty<Parameter>();

            foreach (var parameter in parameters)
            {
                yield return parameter;
            }
        }
    }

    public override IEnumerable<Parameter> VisitProperty(PropertyInfo property)
    {
        var parameterAttribute = property.GetCustomAttribute<ParameterAttribute>();

        if (parameterAttribute == null) yield break;

        var name = !string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : property.Name;

        yield return new Parameter(name, parameterAttribute.Description,property);
    }
}
#endif