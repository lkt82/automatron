#if NET8_0
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
                foreach (var pipeline in type.Accept(this) ?? [])
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

            var yamlName = (!string.IsNullOrEmpty(pipelineAttribute.YmlName) ? pipelineAttribute.YmlName : name) + ".yml";

            var pipeline = new Pipeline(name, p => new StageVisitor(p).VisitTypes(type.GetAllNestedTypes().Append(type)), type)
            {
                DisplayName = pipelineAttribute.DisplayName,
                YmlName = yamlName,
                YmlDir = pipelineAttribute.YmlDir,
                RootDir = pipelineAttribute.RootDir
            };

            pipeline.Variables.UnionWith(type.Accept(new VariableVisitor()) ?? []);
            pipeline.Parameters.UnionWith(type.Accept(new ParameterVisitor()) ?? []);

            return pipeline;

        }
    }
}
#endif
