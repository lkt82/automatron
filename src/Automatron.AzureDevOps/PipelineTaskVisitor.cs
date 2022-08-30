#if NET6_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Automatron.AzureDevOps.Annotations;
using Automatron.Reflection;

namespace Automatron.AzureDevOps
{
    internal class PipelineTaskVisitor : TaskVisitor
    {
        private readonly IDictionary<string, IDictionary<string, object?>> _templateParameters = new Dictionary<string, IDictionary<string, object?>>();

        public PipelineTaskVisitor(ITypeProvider allowTypes) : base(allowTypes)
        {
        }

        protected override IEnumerable<Attribute> GetCustomAttributes(MemberInfo member)
        {
            var attributes = new List<Attribute>();

            Attribute? attribute = member.GetCachedCustomAttribute<VariableAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
            }

            attribute = member.GetCachedCustomAttribute<PipelineAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedCustomAttribute<StageAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedCustomAttribute<JobAttribute>();

            if (attribute != null)
            {
                attributes.Add(attribute);
                return attributes;
            }

            attribute = member.GetCachedCustomAttribute<StepAttribute>();

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
                case DeploymentJobAttribute jobAttribute:
                    VisitDeploymentJobAttribute(jobAttribute);
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
                case ParameterAttribute parameterAttribute:
                    VisitParameterAttribute(parameterAttribute);
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

            var task = new Task(GetName(methodInfo, stepAttribute.Name), new HashSet<Task>(), new MethodAction(methodInfo, type), parameters)
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

            var task = new Task(GetName(type, jobAttribute.Name), new HashSet<Task>(), new EmptyAction(type), parameters)
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

            AddTemplateParameterValues(typeId,type);

            Tasks.Add(typeId, task);
            ParentTask?.Dependencies.Add(task);

            Task = task;
            ParentTask = task;
        }

        private void VisitDeploymentJobAttribute(DeploymentJobAttribute jobAttribute)
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(type, jobAttribute.Name), new HashSet<Task>(), new EmptyAction(type), parameters)
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

            var runtimeParameters = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(jobAttribute.Name))
            {
                runtimeParameters.Add(nameof(jobAttribute.Environment), jobAttribute.Environment);
            }
         
            AddTemplateParameterValues(typeId,type, runtimeParameters);

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

            var task = new Task(GetName(type, stageAttribute.Name), new HashSet<Task>(), new EmptyAction(type), parameters)
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

            AddTemplateParameterValues(typeId,type);

            Tasks.Add(typeId, task);
            ParentTask?.Dependencies.Add(task);

            Task = task;
            ParentTask = task;
        }

        private void AddTemplateParameterValues(string id, MemberInfo type)
        {
            AddTemplateParameterValues(id, type, new Dictionary<string, object?>());
        }

        private void AddTemplateParameterValues(string id, MemberInfo type, Dictionary<string, object?> runtimeParameters)
        {
            var parameterAttributes = type.GetCachedCustomAttributes<ParameterAttribute>();

            foreach (var parameterAttribute in parameterAttributes)
            {
                if (string.IsNullOrEmpty(parameterAttribute.Name))
                {
                    continue;
                }

                runtimeParameters[parameterAttribute.Name] = parameterAttribute.Value;
            }

            foreach (var runtimeParameter in runtimeParameters)
            {
                if (ParentType != null && runtimeParameter.Value is string strValue)
                {
                    var match = Regex.Match(strValue, "^\\$\\{\\{(?<name>.+)\\}\\}");
                    if (match.Success)
                    {
                        if (_templateParameters.TryGetValue(GetPath(ParentType), out var parameters))
                        {
                            runtimeParameters[runtimeParameter.Key] = parameters[match.Groups["name"].Value];
                        }
                    }
                }
            }

            if (runtimeParameters.Any())
            {
                _templateParameters.Add(id, runtimeParameters);
            }
        }

        private void VisitPipelineAttribute(PipelineAttribute pipelineAttribute)
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(type, pipelineAttribute.Name), new HashSet<Task>(), new EmptyAction(type), parameters)
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

            var descriptionAttribute = property.GetCachedCustomAttribute<DescriptionAttribute>();

            //var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : property.Name;
            var tokens = new HashSet<string>();

            if (ParentTask != null)
            {
                tokens.Add(ParentTask.Name);
            }

            tokens.Add(!string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : property.Name);

            var name = string.Join('-', tokens);

            AddParameter(new RuntimeParameter(name, descriptionAttribute?.Description, property.PropertyType), property);
        }

        private void VisitParameterAttribute(ParameterAttribute parameterAttribute)
        {
            var property = Property;
            var type = Type!;

            if (property == null)
            {
                return;
            }

            var name = !string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : property.Name;

            var tokens = new HashSet<string>();

            if (ParentTask != null)
            {
                tokens.Add(ParentTask.Name);
            }

            tokens.Add(name);

            var parameterName = string.Join('-', tokens);

            var typeId = GetPath(type);

            if (!type.IsNested)
            {
                AddParameter(new RuntimeParameter(parameterName, parameterAttribute.DisplayName, property.PropertyType), property);
                return;
            }

            if (!_templateParameters.TryGetValue(typeId, out var parameters))
            {
                return;
            }

            if (!parameters.TryGetValue(name, out var value))
            {
                return;
            }

            AddParameter(new ComputedParameter(parameterName, property.PropertyType, value), property);
        }
    }
}
#endif