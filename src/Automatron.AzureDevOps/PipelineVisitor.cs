#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.Reflection;

namespace Automatron.AzureDevOps
{
    internal class PipelineVisitor : SymbolVisitor
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineVisitor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Dictionary<string, ITask> Tasks { get; } = new();

        private readonly HashSet<object> _visited = new();

        private MethodInfo? CurrentMethod { get; set; }

        private Dictionary<MemberInfo, Dictionary<Type,Attribute>> Cache { get; } = new();

        private T? GetCachedAttribute<T>(MemberInfo member, bool inherit=true) where T : Attribute
        {
            if (Cache.TryGetValue(member, out var typeCached))
            {
                if (typeCached.TryGetValue(typeof(T), out var attribute))
                {
                    return (T)attribute;
                }

                var customAttribute2 = member.GetCustomAttribute<T>(inherit);
                if (customAttribute2 != null)
                {
                    typeCached.Add(typeof(T), customAttribute2);
                }
                return customAttribute2;
            }

            var customAttribute = member.GetCustomAttribute<T>(inherit);
            if (customAttribute != null)
            {
                Cache.Add(member, new Dictionary<Type, Attribute> { {typeof(T), customAttribute } });
            }

            return customAttribute;
        }

        private Type? CurrentType { get; set; }

        private MemberInfo? CurrentMember { get; set; }

        private Task? Pipeline { get; set; }

        private Task? Stage { get; set; }

        private Task? Job { get; set; }

        public override void VisitType(Type type)
        {
            if (_visited.Contains(type))
            {
                return;
            }

            _visited.Add(type);

            CurrentType = type;
            CurrentMember = type;

            var types = type.AsFlat().ToArray();

            if (!types.Any(c => Attribute.IsDefined(c, typeof(PipelineAttribute),false) ||
                                 Attribute.IsDefined(c, typeof(StageAttribute), false) ||
                                 Attribute.IsDefined(c, typeof(JobAttribute), false) ||
                                 Attribute.IsDefined(c, typeof(DeploymentJobAttribute), false))
               )
            {
                return;
            }

            foreach (var attribute in types.SelectMany(c=> c.GetCustomAttributesData()))
            {
                attribute.Accept(this);
            }

            foreach (var method in types.SelectMany(c => c.GetMethods(BindingFlags.Instance| BindingFlags.Public | BindingFlags.DeclaredOnly)))
            {
                method.Accept(this);
            }
        }

        public override void VisitMethod(MethodInfo methodInfo)
        {
            if (_visited.Contains(methodInfo))
            {
                return;
            }

            _visited.Add(methodInfo);

            CurrentMethod = methodInfo;
            CurrentMember = methodInfo;

            if (!Attribute.IsDefined(methodInfo, typeof(StepAttribute)))
            {
                return;
            }

            foreach (var attribute in methodInfo.GetCustomAttributesData())
            {
                attribute.Accept(this);
            }
        }

        public string GetName(MemberInfo member)
        {
            var reflectedType = member;

            var tokens = new List<string>();

            while (reflectedType != null)
            {
                var name = GetMemberName(reflectedType);
                if (!string.IsNullOrEmpty(name))
                {
                    tokens.Add(name);
                }

                reflectedType = reflectedType.ReflectedType;
            }

            tokens.Reverse();

            return string.Join('-', tokens);
        }

        private string? GetMemberName(MemberInfo memberInfo)
        {
            var name = memberInfo.Name;

            var attributes = memberInfo.GetCustomAttributesData();

            if (memberInfo is Type type)
            {
                attributes = type.AsFlat().SelectMany(c => c.GetCustomAttributesData()).ToList();
            }

            foreach (var attributeData in attributes)
            {
                if (attributeData.AttributeType == typeof(PipelineAttribute))
                {
                    var attribute = GetCachedAttribute<PipelineAttribute>(memberInfo, true)!;
                    return string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                }

                if (attributeData.AttributeType == typeof(StageAttribute))
                {
                    var attribute = GetCachedAttribute<StageAttribute>(memberInfo, true)!;
                    return string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                }

                if (attributeData.AttributeType == typeof(JobAttribute))
                {
                    var attribute = GetCachedAttribute<JobAttribute>(memberInfo, true)!;
                    return string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                }

                if (attributeData.AttributeType == typeof(DeploymentJobAttribute))
                {
                    var attribute = GetCachedAttribute<DeploymentJobAttribute>(memberInfo, true)!;
                    return string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                }

                if (attributeData.AttributeType == typeof(StepAttribute))
                {
                    var attribute = GetCachedAttribute<StepAttribute>(memberInfo, true)!;
                    return string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                }
            }

            return null;
        }

        private static string GetKey(MemberInfo methodInfo)
        {
            return methodInfo.ReflectedType!.FullName + "." + methodInfo.Name;
        }

        private static string GetKey(Type type)
        {
            return type.FullName!;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            switch (attribute)
            {
                case DeploymentJobAttribute deploymentJob2Attribute:
                    VisitDeploymentJobAttribute(deploymentJob2Attribute);
                    break;
                case StageAttribute stageAttribute:
                    VisitStageAttribute(stageAttribute);
                    break;
                case PipelineAttribute pipelineAttribute:
                    VisitPipelineAttribute(pipelineAttribute);
                    break;
                case StepAttribute stepAttribute:
                    VisitStepAttribute(stepAttribute);
                    break;
            }
        }

        public override void VisitAttributeData(CustomAttributeData attributeData)
        {
            if (attributeData.AttributeType == typeof(JobAttribute))
            {
                GetCachedAttribute<JobAttribute>(CurrentMember!, true)!.Accept(this);
            }
            else if (attributeData.AttributeType == typeof(DeploymentJobAttribute))
            {
                GetCachedAttribute<DeploymentJobAttribute>(CurrentMember!, true)!.Accept(this);
            }
            else if(attributeData.AttributeType == typeof(StageAttribute))
            {
                GetCachedAttribute<StageAttribute>(CurrentMember!, true)!.Accept(this);
            }
            else if(attributeData.AttributeType == typeof(PipelineAttribute))
            {
                GetCachedAttribute<PipelineAttribute>(CurrentMember!, true)!.Accept(this);
            }
            else if(attributeData.AttributeType == typeof(StepAttribute))
            {
                GetCachedAttribute<StepAttribute>(CurrentMember!, true)!.Accept(this);
            }
        }

        public virtual void VisitPipelineAttribute(PipelineAttribute pipelineAttribute)
        {
            var type = CurrentType!;

            var key = GetKey(type);

            ITask pipelineTask = new ShallowTask(GetName(type), new HashSet<ITask>())
            {
                Default = false
            };

            Tasks.Add(key, pipelineTask);

            Pipeline = pipelineTask;
        }

        public virtual void VisitDeploymentJobAttribute(DeploymentJobAttribute deploymentJobAttribute)
        {
            var type = CurrentType!;

            var key = GetKey(type);

            if (Tasks.ContainsKey(key))
            {
                Job = Stage;

                return;
            }

            var job = new ShallowTask(GetName(type), new HashSet<ITask>())
            {
                Default = false
            };

            Tasks.Add(key, job);

            Job = job;
            Stage?.Dependencies.Add(job);

            if (deploymentJobAttribute.DependsOn != null)
            {
                foreach (var jobType in deploymentJobAttribute.DependsOn)
                {
                    jobType.Accept(this);

                    var dependentKey = GetKey(jobType);

                    var dependentJob= Tasks[dependentKey];

                    job.Dependencies.Add(dependentJob);
                }
            }
        }

        public virtual void VisitStageAttribute(StageAttribute stageAttribute)
        {
            var type = CurrentType!;

            var key = GetKey(type);

            if (Tasks.ContainsKey(key))
            {
                Stage = Pipeline;

                return;
            }

            var stage = new ShallowTask(GetName(type), new HashSet<ITask>())
            {
                Default = false
            };

            Tasks.Add(key, stage);

            Stage = stage;
            Pipeline?.Dependencies.Add(stage);

            if (stageAttribute.DependsOn != null)
            {
                foreach (var stageType in stageAttribute.DependsOn)
                {
                    stageType.Accept(this);

                    var dependentKey = GetKey(stageType);

                    var dependentStage = Tasks[dependentKey];

                    stage.Dependencies.Add(dependentStage);
                }
            }
        }

        public virtual void VisitStepAttribute(StepAttribute stepAttribute)
        {
            var methodInfo = CurrentMethod!;

            var key = GetKey(methodInfo);

            var type = methodInfo.ReflectedType!.IsInterface ? CurrentType! : methodInfo.ReflectedType;

            var step = new MethodTask(GetName(methodInfo), _serviceProvider, type, methodInfo, new HashSet<ITask>())
            {
                Default = false
            };

            Tasks.Add(key, step);

            Job?.Dependencies.Add(step);

            if (stepAttribute.DependsOn != null)
            {
                var dependentType = type;

                foreach (var methodName in stepAttribute.DependsOn)
                {
                    var dependentKey = GetKey(dependentType.GetMethod(methodName) ?? throw new InvalidOperationException());

                    var task = Tasks[dependentKey];

                    step.Dependencies.Add(task);
                }
            }
        }
    }
}
#endif