#if NET8_0
using System;
using System.Linq;
using System.Reflection;
using Automatron.Reflection;
using Automatron.Tasks.Annotations;

namespace Automatron.Tasks.Middleware;

internal class TaskTypeVisitor : MemberVisitor<bool>
{
    public override bool VisitType(Type type)
    {
        var isTask = type.GetAllCustomAttribute<TaskAttribute>() != null;

        var hasTasks = type.GetAllMethods().Any(c => c.Accept(this));

        var hasParameters = type.GetAllProperties().Any(c => c.Accept(this));

        return isTask || hasTasks || hasParameters;
    }

    public override bool VisitMethod(MethodInfo methodInfo)
    {
        return methodInfo.GetAllCustomAttribute<TaskAttribute>() != null;
    }

    public override bool VisitProperty(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetAllCustomAttribute<ParameterAttribute>() != null;
    }
}
#endif