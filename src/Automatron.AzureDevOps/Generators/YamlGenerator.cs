using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Converters;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.String;

namespace Automatron.AzureDevOps.Generators
{
    [Generator]
    public class YamlGenerator : ISourceGenerator
    {
        private readonly ISerializer _serializer;

        public YamlGenerator()
        {
            _serializer = CreateYamlSerializer();
        }

        public static T MapToType<T>(AttributeData attributeData) where T : Attribute
        {
            return MapToType<T>(attributeData,typeof(T));
        }

        public static T MapToType<T>(AttributeData attributeData,Type type) where T : Attribute
        {
            T attribute;
            if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
            {
                attribute = (T)Activator.CreateInstance(type, GetActualConstructorParams(attributeData).ToArray())!;
            }
            else
            {
                attribute = (T)Activator.CreateInstance(type)!;
            }
            // ReSharper disable once UseDeconstruction
            foreach (var keyValue in attributeData.NamedArguments)
            {
                var property = type.GetProperty(keyValue.Key);

                if (property == null)
                {
                    throw new Exception();
                }

                property.SetValue(attribute, GetTypedValue(keyValue.Value));
            }
            return attribute;
        }

        private static object GetTypedValue(TypedConstant typedConstant)
        {
            if (typedConstant.Type != null && 
                typedConstant.Kind != TypedConstantKind.Primitive && 
                typedConstant.Kind != TypedConstantKind.Array &&
                typedConstant.Kind != TypedConstantKind.Enum)
                throw new NotSupportedException();

            var typeString = typedConstant.Type!.ToString();

            if (typedConstant.Kind == TypedConstantKind.Enum)
            {
                typeString = typedConstant.Type!.BaseType!.ToString();
            }

            return typeString switch
            {
                "string[]" => typedConstant.Values.Select(a => a.Value).OfType<string>().ToArray(),
                "int[]" => typedConstant.Values.Select(a => a.Value).OfType<int>().ToArray(),
                "DateTime[]" => typedConstant.Values.Select(a => a.Value).OfType<DateTime>().ToArray(),
                "bool[]" => typedConstant.Values.Select(a => a.Value).OfType<bool>().ToArray(),
                "string" => typedConstant.Value!,
                "bool" => typedConstant.Value!,
                "int" => typedConstant.Value!,
                "DateTime" => typedConstant.Value!,
                "System.Enum" => Enum.ToObject(Type.GetType(typedConstant.Type!.ToString()!)!,typedConstant.Value!),
                _ => throw new NotSupportedException()
            };
        }

        public static IEnumerable<object?> GetActualConstructorParams(AttributeData attributeData)
        {
            return attributeData.ConstructorArguments.Select(GetTypedValue);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Execute code generator");

            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            var types = new List<INamedTypeSymbol>();

            var currentType = mainMethod!.ContainingType;

            while (currentType!.ToString() != "object")
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }

            types.AddRange(mainMethod.ContainingType.AllInterfaces);

            var typeAttributes = types.SelectMany(c => c.GetAttributes()).ToArray();

            var typePipelineAttributes = typeAttributes.Where(c => c.AttributeClass!.Name == nameof(PipelineAttribute)).Select(MapToType<PipelineAttribute>).ToList();
            var typeCiTriggerAttributes = typeAttributes.Where(c => c.AttributeClass!.Name == nameof(CiTriggerAttribute)).Select(MapToType<CiTriggerAttribute>).ToList();
            var typeScheduledTriggerAttributes = typeAttributes.Where(c => c.AttributeClass!.Name == nameof(ScheduledTriggerAttribute)).Select(MapToType<ScheduledTriggerAttribute>).ToList();
            var variableGroupAttributes = typeAttributes.Where(c => c.AttributeClass!.Name == nameof(VariableGroupAttribute)).Select(MapToType<VariableGroupAttribute>).ToList();
            var variableAttributes = typeAttributes.Where(c => c.AttributeClass!.Name == nameof(VariableAttribute)).Select(MapToType<VariableAttribute>).ToList();

            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory",
                    out var projectDirectory))
            {
                throw new InvalidOperationException();
            }

            var members = types.SelectMany(c=> c.GetMembers()).Where(c=> c.Kind == SymbolKind.Method && c.DeclaredAccessibility == Accessibility.Public).ToArray();

            foreach (var pipelineAttribute in typePipelineAttributes)
            {
                var stageLookUp = new Dictionary<string, Stage>();

                var jobLookUp = new Dictionary<string, IJob>();

                var pipeline = CreatePipeline(pipelineAttribute);

                CreateCiTrigger(typeCiTriggerAttributes, pipelineAttribute, pipeline);

                CreateVariables(variableGroupAttributes, pipelineAttribute, variableAttributes, pipeline);

                CreateScheduledTriggers(typeScheduledTriggerAttributes, pipelineAttribute, pipeline);

                CreateStages(members, pipelineAttribute, pipeline, stageLookUp);

                CreateJobs(members, stageLookUp, jobLookUp);

                CreateSteps(members, jobLookUp);

                WritePipeline(pipeline, projectDirectory);
            }
        }

        private static void CreateStages(IEnumerable<ISymbol> members, PipelineAttribute pipelineAttribute, Pipeline pipeline,
            IDictionary<string, Stage> stageLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetAttributes())
                {
                    CreateStage(attribute, pipelineAttribute, member, pipeline, stageLookUp);
                }
            }
        }

        private static void CreateJobs(IEnumerable<ISymbol> members, IReadOnlyDictionary<string, Stage> stageLookUp, IDictionary<string, IJob> jobLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetAttributes())
                {
                    CreateJob(attribute, member, stageLookUp, jobLookUp);
                }

                foreach (var attribute in member.GetAttributes())
                {
                    CreateDeploymentJob(attribute, member, stageLookUp, jobLookUp);
                }
            }
        }

        private static void CreateSteps(IEnumerable<ISymbol> members, IReadOnlyDictionary<string, IJob> jobLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetAttributes())
                {
                    CreateStep(attribute, member, jobLookUp);
                }
            }
        }

        private static void CreateStep(AttributeData attribute, ISymbol member, IReadOnlyDictionary<string, IJob> jobLookUp)
        {
            if (attribute.AttributeClass!.BaseType!.Name != nameof(StepAttribute))
            {
                return;
            }

            var stepTypeName = attribute.AttributeClass + ", " + attribute.AttributeClass.ContainingAssembly;

            var stepType = Type.GetType(stepTypeName);

            var stepAttribute = MapToType<StepAttribute>(attribute, stepType ?? throw new InvalidOperationException());

            var jobName = !IsNullOrEmpty(stepAttribute.Job) ? stepAttribute.Job! : member.Name;

            if (!jobLookUp.TryGetValue(jobName, out var job))
            {
                return;
            }

            var step = stepAttribute.Create(member);

            job.Steps.Add(step);
        }

        private static void CreateStage(AttributeData attribute, PipelineAttribute pipelineAttribute, ISymbol member,
            Pipeline pipeline, IDictionary<string, Stage> stageLookUp)
        {
            if (attribute.AttributeClass!.Name != nameof(StageAttribute))
            {
                return;
            }

            var stageAttribute = MapToType<StageAttribute>(attribute);
            if (stageAttribute.Pipeline != pipelineAttribute.Name && stageAttribute.Pipeline != null)
            {
                return;
            }

            var stage = CreateStage(stageAttribute, member);

            pipeline.Stages.Add(stage);
            stageLookUp[stage.Name] = stage;
        }

        private static void CreateDeploymentJob(AttributeData attribute, ISymbol member, IReadOnlyDictionary<string, Stage> stageLookUp,
            IDictionary<string, IJob> jobLookUp)
        {
            if (attribute.AttributeClass!.Name != nameof(DeploymentJobAttribute))
            {
                return;
            }

            var jobAttribute = MapToType<DeploymentJobAttribute>(attribute);

            var stageName = !IsNullOrEmpty(jobAttribute.Stage) ? jobAttribute.Stage! : member.Name;

            if (!stageLookUp.TryGetValue(stageName, out var stage))
            {
                return;
            }

            var job = CreateDeploymentJob(member, jobAttribute);

            stage.Jobs.Add(job);
            jobLookUp[job.Name] = job;
        }

        private static void CreateJob(AttributeData attribute, ISymbol member, IReadOnlyDictionary<string, Stage> stageLookUp, IDictionary<string, IJob> jobLookUp)
        {
            if (attribute.AttributeClass!.Name != nameof(JobAttribute))
            {
                return;
            }

            var jobAttribute = MapToType<JobAttribute>(attribute);

            var stageName = !IsNullOrEmpty(jobAttribute.Stage) ? jobAttribute.Stage! : member.Name;

            if (!stageLookUp.TryGetValue(stageName, out var stage))
            {
                return;
            }

            var job = CreateJob(member, jobAttribute);

            stage.Jobs.Add(job);
            jobLookUp[job.Name] = job;
        }

        private static void CreateScheduledTriggers(IEnumerable<ScheduledTriggerAttribute> typeScheduledTriggerAttributes, PipelineAttribute pipelineAttribute,
            Pipeline pipeline)
        {
            var scheduledTriggerAttributes =
                typeScheduledTriggerAttributes.Where(c => c.Pipeline == pipelineAttribute.Name || c.Pipeline == null);

            foreach (var scheduledTriggerAttribute in scheduledTriggerAttributes)
            {
                CreateScheduledTrigger(pipeline, scheduledTriggerAttribute);
            }
        }

        private static void CreateCiTrigger(IEnumerable<CiTriggerAttribute> typeCiTriggerAttributes, PipelineAttribute pipelineAttribute,
            Pipeline pipeline)
        {
            var ciTriggerAttribute =
                typeCiTriggerAttributes.FirstOrDefault(c => c.Pipeline == pipelineAttribute.Name || c.Pipeline == null);

            if (ciTriggerAttribute != null)
            {
                CreateCiTrigger(pipeline, ciTriggerAttribute);
            }
        }

        private static void CreateVariables(IEnumerable<VariableGroupAttribute> variableGroupAttributes,
            PipelineAttribute pipelineAttribute,
            IEnumerable<VariableAttribute> variableAttributes,
            Pipeline pipeline)
        {
            foreach (var variableGroupAttribute in variableGroupAttributes.Where(c =>
                         c.Pipeline == pipelineAttribute.Name || c.Pipeline == null))
            {
                pipeline.Variables.Add(new VariableGroup(variableGroupAttribute.Name));
            }

            foreach (var variableAttribute in variableAttributes.Where(c =>
                         c.Pipeline == pipelineAttribute.Name || c.Pipeline == null))
            {
                pipeline.Variables.Add(new Variable(variableAttribute.Name, variableAttribute.Value));
            }
        }

        private static Stage CreateStage(StageAttribute stageAttribute, ISymbol member)
        {
            var name = !IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name! : member.Name;

            var stage = new Stage(name, stageAttribute.DisplayName, stageAttribute.DependsOn);
            return stage;
        }

        private static Pipeline CreatePipeline(PipelineAttribute pipelineAttribute)
        {
            var pipeline = new Pipeline(pipelineAttribute.Name, pipelineAttribute.Path, pipelineAttribute.YmlName,
                pipelineAttribute.YmlPath);
            return pipeline;
        }

        private static Job CreateJob(ISymbol member, JobAttribute jobAttribute)
        {
            var name = !IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

            var job = new Job(name, jobAttribute.DisplayName, jobAttribute.DependsOn);
            return job;
        }

        private static DeploymentJob CreateDeploymentJob(ISymbol member, DeploymentJobAttribute jobAttribute)
        {
            var name = !IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

            var job = new DeploymentJob(name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Environment)
            {
                TimeoutInMinutes = jobAttribute.TimeoutInMinutes == default
                    ? null
                    : jobAttribute.TimeoutInMinutes
            };
            return job;
        }

        private static void CreateScheduledTrigger(Pipeline pipeline, ScheduledTriggerAttribute scheduledTriggerAttribute)
        {
            var scheduledTrigger = new ScheduledTrigger(scheduledTriggerAttribute.Cron)
            {
                Always = scheduledTriggerAttribute.Always,
                DisplayName = scheduledTriggerAttribute.DisplayName
            };

            if (scheduledTriggerAttribute.IncludeBranches != null || scheduledTriggerAttribute.ExcludeBranches != null)
            {
                scheduledTrigger.Branches = new TriggerBranches
                {
                    Include = scheduledTriggerAttribute.IncludeBranches,
                    Exclude = scheduledTriggerAttribute.ExcludeBranches
                };
            }
            pipeline.Schedules.Add(scheduledTrigger);
        }

        private static void CreateCiTrigger(Pipeline pipeline, CiTriggerAttribute ciTriggerAttribute)
        {
            if (ciTriggerAttribute.Disabled)
            {
                pipeline.CiTrigger = new DisabledCiTrigger();
            }
            else if (ciTriggerAttribute.IncludeBranches != null || ciTriggerAttribute.ExcludeBranches != null)
            {
                pipeline.CiTrigger = new CiTrigger
                {
                    Batch = ciTriggerAttribute.Batch,
                    Branches = new TriggerBranches
                    {
                        Include = ciTriggerAttribute.IncludeBranches,
                        Exclude = ciTriggerAttribute.ExcludeBranches
                    }
                };
            }
        }

        private static ISerializer CreateYamlSerializer()
        {
            var disabledCiTriggerConverter = new DisabledCiTriggerConverter();
            var serializerBuilder = new SerializerBuilder();

            var serializer = serializerBuilder.WithTypeConverter(disabledCiTriggerConverter)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
                .Build();
            return serializer;
        }

        private void WritePipeline(Pipeline pipeline, string projectDirectory)
        {
            var dir = Path.Combine(projectDirectory, pipeline.YmlPath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using var stream = File.CreateText(Path.Combine(dir, pipeline.YmlName));
            _serializer.Serialize(stream, pipeline);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif
            Debug.WriteLine("Initialize code generator");
        }
    }
}
