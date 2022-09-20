#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Models;
using Automatron.Reflection;

namespace Automatron.AzureDevOps.Middleware
{
    internal class PipelineVisitor : MemberVisitor<IEnumerable<Pipeline>>
    {
        public IEnumerable<Pipeline> VisitTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                foreach (var pipeline in type.Accept(this) ?? Enumerable.Empty<Pipeline>())
                {
                    yield return pipeline;
                }
            }
        }

        public override IEnumerable<Pipeline>? VisitType(Type type)
        {
            var pipelineAttribute = type.GetAllCustomAttribute<PipelineAttribute>();

            if (pipelineAttribute != null)
            {
                yield return CreatePipeline(type, pipelineAttribute);
            }
        }

        private static Pipeline CreatePipeline(Type type, PipelineAttribute pipelineAttribute)
        {
            var name = !string.IsNullOrEmpty(pipelineAttribute.Name) ? pipelineAttribute.Name : type.Name;

            var pipeline = new Pipeline(name, p => new StageVisitor(p).VisitTypes(type.GetAllNestedTypes().Append(type)), type);

            pipeline.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? Enumerable.Empty<Variable>());
            
            return pipeline;

        }
    }
}
#endif
