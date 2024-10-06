#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class TemplateParameterVisitor : MemberVisitor<IEnumerable<TemplateParameter>>
{
    public override IEnumerable<TemplateParameter>? VisitType(Type type)
    {
        foreach (var propertyInfo in type.GetAllProperties())
        {
            foreach (var templateParameter in propertyInfo.Accept(this) ?? Enumerable.Empty<TemplateParameter>())
            {
                yield return templateParameter;
            }
        }
    }

    public override IEnumerable<TemplateParameter>? VisitProperty(PropertyInfo propertyInfo)
    {
        var variableAttribute = propertyInfo.GetAllCustomAttribute<TemplateParameterAttribute>();

        if (variableAttribute != null)
        {
            yield return CreateTemplateParameter(propertyInfo, variableAttribute);
        }
    }

    private static TemplateParameter CreateTemplateParameter(PropertyInfo propertyInfo, TemplateParameterAttribute variableAttribute)
    {
        var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : propertyInfo.Name;

        return new TemplateParameter(name, propertyInfo);
    }

}
#endif