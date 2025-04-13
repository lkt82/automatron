#if NET8_0
using System;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware;

internal class PipelineTypeVisitor : MemberVisitor<bool>
{
    public override bool VisitType(Type type)
    {
        var isPipeline = type.GetAllCustomAttribute<PipelineAttribute>() != null;

        var isStage = type.GetAllCustomAttribute<StageAttribute>() != null;

        var isJob = type.GetAllCustomAttribute<JobAttribute>() != null;

        var hasSteps = type.GetAllMethods().Any(c => c.Accept(this));

        var hasVariables = type.GetAllProperties().Any(c => c.Accept(this));

        return isPipeline || isStage || isJob  || hasSteps || hasVariables;
    }

    public override bool VisitMethod(MethodInfo methodInfo)
    {
        return methodInfo.GetAllCustomAttribute<StepAttribute>() != null;
    }

    public override bool VisitProperty(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetAllCustomAttribute<VariableAttribute>() != null;
    }
}
#endif