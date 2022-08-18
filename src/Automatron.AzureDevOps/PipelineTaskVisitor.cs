#if NET6_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.Reflection;

namespace Automatron.AzureDevOps
{
    internal class PipelineTaskVisitor : TaskVisitor
    {
        public PipelineTaskVisitor(ITypeProvider allowTypes) : base(allowTypes)
        {
        }

        protected override IEnumerable<Attribute> GetAttributes(MemberInfo member)
        {
            var attributes = new List<Attribute>();

            Attribute? attribute = member.GetCachedAttribute<VariableAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
            }

            attribute = member.GetCachedAttribute<PipelineAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedAttribute<StageAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedAttribute<JobAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedAttribute<StepAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            return attributes;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            switch (attribute)
            {
                case PipelineAttribute pipelineAttribute:
                    VisitPipelineAttribute(pipelineAttribute);
                    break;
                case StageAttribute stageAttribute:
                    VisitStageAttribute(stageAttribute);
                    break;
                case JobAttribute jobAttribute:
                    VisitJobAttribute(jobAttribute);
                    break;
                case StepAttribute stepAttribute:
                    VisitStepAttribute(stepAttribute);
                    break;
                case VariableAttribute variableAttribute:
                    VisitVariableAttribute(variableAttribute);
                    break;
            }
        }

        private void VisitStepAttribute(StepAttribute stepAttribute)
        {
            var methodInfo = MethodInfo!;

            var type = methodInfo.ReflectedType!.IsInterface || methodInfo.ReflectedType!.IsAbstract ? Type! : methodInfo.ReflectedType;
            
            var methodId = GetPath(methodInfo);
            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(methodInfo, stepAttribute.Name), new HashSet<Task>(), new MethodActionDescriptor(methodInfo, type), parameters)
            {
                Default = false
            };

            if (stepAttribute.DependsOn != null)
            {
                foreach (var dependentStep in stepAttribute.DependsOn)
                {
                    var dependentKey = GetPath(type.GetMethod(dependentStep) ?? throw new InvalidOperationException());

                    var dependency = Tasks[dependentKey];

                    task.Dependencies.Add(dependency);
                }
            }

            Tasks.Add(methodId, task);
            ParentTask?.Dependencies.Add(task);

            Task = task;
        }

        private void VisitJobAttribute(JobAttribute jobAttribute)
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(type, jobAttribute.Name), new HashSet<Task>(), new EmptyActionDescriptor(type), parameters)
            {
                Default = false
            };

            if (jobAttribute.DependsOn != null)
            {
                foreach (var dependentJob in jobAttribute.DependsOn.Cast<Type>())
                {
                    var dependentKey = GetPath(dependentJob ?? throw new InvalidOperationException());

                    var dependency = Tasks[dependentKey];

                    task.Dependencies.Add(dependency);
                }
            }

            Tasks.Add(typeId, task);
            ParentTask?.Dependencies.Add(task);

            Task = task;
            ParentTask = task;
        }

        private void VisitStageAttribute(StageAttribute stageAttribute)
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(type, stageAttribute.Name), new HashSet<Task>(), new EmptyActionDescriptor(type), parameters)
            {
                Default = false
            };

            if (stageAttribute.DependsOn != null)
            {
                foreach (var dependentStage in stageAttribute.DependsOn.Cast<Type>())
                {
                    var dependentKey = GetPath(dependentStage ?? throw new InvalidOperationException());

                    var dependency = Tasks[dependentKey];

                    task.Dependencies.Add(dependency);
                }
            }

            Tasks.Add(typeId, task);
            ParentTask?.Dependencies.Add(task);

            Task = task;
            ParentTask = task;
        }

        private void VisitPipelineAttribute(PipelineAttribute pipelineAttribute)
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(type, pipelineAttribute.Name), new HashSet<Task>(), new EmptyActionDescriptor(type), parameters)
                {
                    Default = false
                };
            

            Tasks.Add(typeId, task);

            Task = task;
            ParentTask = task;
        }

        private void VisitVariableAttribute(VariableAttribute variableAttribute)
        {
            var property = Property;

            if (property == null)
            {
                return;
            }

            //var tokens = new HashSet<string>();

            //if (ParentTask != null)
            //{
            //    tokens.Add(ParentTask.Name);
            //}

            //tokens.Add(!string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : property.Name);

            //var name = string.Join('-', tokens);

            var descriptionAttribute = property.GetCachedAttribute<DescriptionAttribute>();

            var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : property.Name;

            AddParameter(name, descriptionAttribute?.Description, property);

            //var type = property.ReflectedType!.IsInterface || property.ReflectedType!.IsAbstract ? Type! : property.ReflectedType;

            //var descriptionAttribute = property.GetCachedAttribute<DescriptionAttribute>();

            //var typeId = GetPath(type);

            //var parameter = new Parameter(name, descriptionAttribute?.Description, property.PropertyType);

            //var descriptor = new ParameterDescriptor(parameter, property);

            //if (!TypeParameters.ContainsKey(typeId))
            //{
            //    TypeParameters.Add(typeId, new HashSet<ParameterDescriptor>());
            //}

            //TypeParameters[typeId].Add(descriptor);

            //var propertyId = GetPath(property);

            //Parameters.Add(propertyId, parameter);
        }
    }
}
#endif