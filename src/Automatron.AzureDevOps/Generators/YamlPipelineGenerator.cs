using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    internal class YamlPipelineGenerator : ISourceGenerator
    {
        private readonly ISerializer _serializer;

        public YamlPipelineGenerator()
        {
            _serializer = CreateYamlSerializer();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (Environment.GetEnvironmentVariable("TF_BUILD")?.ToUpperInvariant() == "TRUE")
            {
                return;
            }

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


            var typePipelineAttributes = types.SelectMany(c => c.GetCustomAttributes<PipelineAttribute>()).ToImmutableArray();
            var typeCiTriggerAttributes = types.SelectMany(c => c.GetCustomAttributes<CiTriggerAttribute>()).ToImmutableArray();
            var typeScheduledTriggerAttributes = types.SelectMany(c => c.GetCustomAttributes<ScheduledTriggerAttribute>()).ToImmutableArray();
            var variableGroupAttributes = types.SelectMany(c => c.GetCustomAttributes<VariableGroupAttribute>()).ToImmutableArray();
            var variableAttributes = types.SelectMany(c => c.GetCustomAttributes<VariableAttribute>()).ToImmutableArray();
            var poolAttributes = types.SelectMany(c => c.GetCustomAttributes<PoolAttribute>()).ToImmutableArray();

            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory",
                    out var projectDirectory))
            {
                throw new InvalidOperationException();
            }

            var members = types.SelectMany(c=> c.GetMembers()).Where(c=> c.Kind == SymbolKind.Method && c.DeclaredAccessibility == Accessibility.Public).Cast<IMethodSymbol>().Where(c=> c.MethodKind == MethodKind.Ordinary).ToArray();

            foreach (var pipelineAttribute in typePipelineAttributes)
            {
                var stageLookUp = new Dictionary<string, Stage>();

                var jobLookUp = new Dictionary<string, IJob>();

                var pipeline = CreatePipeline(pipelineAttribute, projectDirectory);

                CreateCiTrigger(typeCiTriggerAttributes, pipeline);

                CreateVariables(variableGroupAttributes, variableAttributes, pipeline);

                CreateScheduledTriggers(typeScheduledTriggerAttributes, pipeline);

                CreatePool(poolAttributes, pipeline);

                CreateStages(members, pipeline, stageLookUp);

                CreateJobs(members, stageLookUp, jobLookUp);

                CreateSteps(members, jobLookUp);

                SavePipeline(pipeline);
            }
        }

        private static void CreatePool(ImmutableArray<PoolAttribute> poolAttributes, Pipeline pipeline)
        {
            var poolAttribute = poolAttributes.FirstOrDefault(c => c.Target == pipeline.Name || c.Target == null);

            if (poolAttribute != null)
            {
                pipeline.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }
        }

        private static void CreateStages(IEnumerable<ISymbol> members, Pipeline pipeline,
            IDictionary<string, Stage> stageLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetCustomAttributes<StageAttribute>())
                {
                    CreateStage(attribute, member, pipeline, stageLookUp);
                }
            }
        }

        private static void CreateJobs(IEnumerable<ISymbol> members, IReadOnlyDictionary<string, Stage> stageLookUp, IDictionary<string, IJob> jobLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetCustomAttributes<JobAttribute>())
                {
                    CreateJob(attribute, member, stageLookUp, jobLookUp);
                }

                foreach (var attribute in member.GetCustomAttributes<DeploymentJobAttribute>())
                {
                    CreateDeploymentJob(attribute, member, stageLookUp, jobLookUp);
                }
            }
        }

        private static void CreateSteps(IEnumerable<ISymbol> members, IReadOnlyDictionary<string, IJob> jobLookUp)
        {
            foreach (var member in members)
            {
                foreach (var attribute in member.GetCustomAbstractAttributes<StepAttribute>())
                {
                    CreateStep(attribute, member, jobLookUp);
                }
            }
        }

        private static void CreateStep(StepAttribute stepAttribute, ISymbol member, IReadOnlyDictionary<string, IJob> jobLookUp)
        {
            var jobName = !IsNullOrEmpty(stepAttribute.Job) ? stepAttribute.Job! : member.Name;

            if (!jobLookUp.TryGetValue(jobName, out var job))
            {
                return;
            }

            var step = stepAttribute.Create(member, job);

            job.Steps.Add(step);
        }

        private static void CreateStage(StageAttribute stageAttribute, ISymbol member,
            Pipeline pipeline, IDictionary<string, Stage> stageLookUp)
        {
            if (stageAttribute.Pipeline != pipeline.Name && stageAttribute.Pipeline != null)
            {
                return;
            }

            var stage = CreateStage(pipeline,stageAttribute, member);

            pipeline.Stages.Add(stage);
            stageLookUp[stage.Name] = stage;
        }

        private static void CreateDeploymentJob(DeploymentJobAttribute jobAttribute, ISymbol member, IReadOnlyDictionary<string, Stage> stageLookUp,
            IDictionary<string, IJob> jobLookUp)
        {

            var stageName = !IsNullOrEmpty(jobAttribute.Stage) ? jobAttribute.Stage! : member.Name;

            if (!stageLookUp.TryGetValue(stageName, out var stage))
            {
                return;
            }

            var job = CreateDeploymentJob(stage,member, jobAttribute);

            stage.Jobs.Add(job);
            jobLookUp[job.Name] = job;
        }

        private static void CreateJob(JobAttribute jobAttribute, ISymbol member, IReadOnlyDictionary<string, Stage> stageLookUp, IDictionary<string, IJob> jobLookUp)
        {
            var stageName = !IsNullOrEmpty(jobAttribute.Stage) ? jobAttribute.Stage! : member.Name;

            if (!stageLookUp.TryGetValue(stageName, out var stage))
            {
                return;
            }

            var job = CreateJob(stage,member, jobAttribute);

            stage.Jobs.Add(job);
            jobLookUp[job.Name] = job;
        }

        private static void CreateScheduledTriggers(IEnumerable<ScheduledTriggerAttribute> typeScheduledTriggerAttributes,
            Pipeline pipeline)
        {
            var scheduledTriggerAttributes =
                typeScheduledTriggerAttributes.Where(c => c.Pipeline == pipeline.Name || c.Pipeline == null);

            foreach (var scheduledTriggerAttribute in scheduledTriggerAttributes)
            {
                CreateScheduledTrigger(pipeline, scheduledTriggerAttribute);
            }
        }

        private static void CreateCiTrigger(IEnumerable<CiTriggerAttribute> typeCiTriggerAttributes,
            Pipeline pipeline)
        {
            var ciTriggerAttribute =
                typeCiTriggerAttributes.FirstOrDefault(c => c.Pipeline == pipeline.Name || c.Pipeline == null);

            if (ciTriggerAttribute != null)
            {
                CreateCiTrigger(pipeline, ciTriggerAttribute);
            }
        }

        private static void CreateVariables(IEnumerable<VariableGroupAttribute> variableGroupAttributes,
            IEnumerable<VariableAttribute> variableAttributes,
            Pipeline pipeline)
        {
            foreach (var variableGroupAttribute in variableGroupAttributes.Where(c =>
                         c.Pipeline == pipeline.Name || c.Pipeline == null))
            {
                pipeline.Variables.Add(new VariableGroup(variableGroupAttribute.Name));
            }

            foreach (var variableAttribute in variableAttributes.Where(c =>
                         c.Pipeline == pipeline.Name || c.Pipeline == null))
            {
                pipeline.Variables.Add(new Variable(variableAttribute.Name, variableAttribute.Value));
            }
        }

        private static Stage CreateStage(Pipeline pipeline,StageAttribute stageAttribute, ISymbol member)
        {
            var name = !IsNullOrEmpty(stageAttribute.Name) ? stageAttribute.Name! : member.Name;

            var stage = new Stage(pipeline,name, stageAttribute.DisplayName, stageAttribute.DependsOn,stageAttribute.Condition);

            var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

            if (poolAttribute != null)
            {
                stage.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }

            return stage;
        }

        private static Pipeline CreatePipeline(PipelineAttribute pipelineAttribute, string projectDirectory)
        {
            var pipeline = new Pipeline(pipelineAttribute.Name, pipelineAttribute.YmlName, pipelineAttribute.YmlPath, pipelineAttribute.RootPath, projectDirectory);
            return pipeline;
        }

        private static Job CreateJob(Stage stage,ISymbol member, JobAttribute jobAttribute)
        {
            var name = !IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

            var job = new Job(stage,name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition);

            if (job.Stage.Name != name)
            {
                var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

                if (poolAttribute != null)
                {
                    job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
                }
            }

            return job;
        }

        private static DeploymentJob CreateDeploymentJob(Stage stage,ISymbol member, DeploymentJobAttribute jobAttribute)
        {
            var name = !IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

            var job = new DeploymentJob(stage,name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition, jobAttribute.Environment)
            {
                TimeoutInMinutes = jobAttribute.TimeoutInMinutes == default
                    ? null
                    : jobAttribute.TimeoutInMinutes
            };

            if (job.Stage.Name != name)
            {
                var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

                if (poolAttribute != null)
                {
                    job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
                }
            }

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
                return;
            }

            var trigger = new CiTrigger
            {
                Batch = ciTriggerAttribute.Batch
            };
        
            if (ciTriggerAttribute.IncludeBranches != null || ciTriggerAttribute.ExcludeBranches != null)
            {
                trigger.Branches = new TriggerBranches
                {
                    Include = ciTriggerAttribute.IncludeBranches,
                    Exclude = ciTriggerAttribute.ExcludeBranches
                };
            }

            if (ciTriggerAttribute.IncludePaths != null || ciTriggerAttribute.ExcludePaths != null)
            {
                trigger.Paths = new TriggerPaths
                {
                    Include = ciTriggerAttribute.IncludePaths,
                    Exclude = ciTriggerAttribute.ExcludePaths
                };
            }
            pipeline.CiTrigger = trigger;
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

        private void SavePipeline(Pipeline pipeline)
        {
            var combined = Path.Combine(pipeline.ProjectDirectory, pipeline.YmlPath);
            var dir = Path.GetFullPath(combined);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var filePath = Path.Combine(dir, pipeline.YmlName);

            using var stream = File.CreateText(filePath);
            _serializer.Serialize(stream, pipeline);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            Debug.WriteLine("Initialize code generator");
        }
    }
}