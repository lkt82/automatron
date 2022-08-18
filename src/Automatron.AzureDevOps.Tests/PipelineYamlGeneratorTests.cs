using System.Collections.Immutable;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators;
using Automatron.AzureDevOps.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using Xunit;

namespace Automatron.AzureDevOps.Tests;

public class PipelineYamlGeneratorTests
{
    private static readonly string ProjectPath = Path.GetFullPath("../../../");

    private const bool Persistent = true;

    private readonly Dictionary<string, string> _options = new()
    {
        {
            "build_property.AzureDevOpsPipelineProjectDirectory", ProjectPath
        },
        { 
            "build_property.AzureDevOpsPipelinePersistent", Persistent.ToString().ToLower()
        }
    };

    [Fact]
    public void Should_be_able_to_map_pipeline()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline) + @""")]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull()
            .And.Subject.Should().ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_ci_trigger()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_ci_trigger) + @""")]
[CiTrigger(Batch = true, IncludeBranches = new[] { ""main"" }, IncludePaths = new[] { ""src"" })]
public class Pipeline
{
    [Step]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull();
        generator.Pipelines.First().CiTrigger.Should().NotBeNull();
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_scheduled_trigger()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_scheduled_trigger) + @""")]
[ScheduledTrigger(
    ""2 0 * * *"",
    DisplayName = ""Midnight"",
    IncludeBranches = new[] { ""master"" }
)]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull();
        generator.Pipelines.First().Schedules.Should().HaveCount(1);
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_stage()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_stage) + @""")]
[Stage]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull();
        generator.Pipelines.First().Stages.Should().NotBeNull();
        generator.Pipelines.First().Stages.Should().HaveCount(1);
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_stage_job()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_stage_job) + @""")]
[Stage]
[Job]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull();
        generator.Pipelines.First().Stages.Should().NotBeNull();
        generator.Pipelines.First().Stages!.First().Jobs.Should().NotBeNull();
        generator.Pipelines.First().Stages!.First().Jobs.Should().HaveCount(1);
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_stage_deployment_job()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_stage_deployment_job) + @""")]
[Stage]
[DeploymentJob(Environment = ""Testing"")]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Should().BeOfType<DeploymentJob>()
            .Subject.Environment.Should().Be("Testing");
    }


    [Fact]
    public void Should_be_able_to_map_pipeline_stage_job_step()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_stage_job_step) + @""")]
[Stage]
[Job]
public class Pipeline
{
    [Step]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle()
            .Subject.Should().BeOfType<AutomatronScript>()
            .Subject.Tasks.Should().Contain("Pipeline-Default");
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_nested_stage()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_nested_stage) + @""")]
public class Pipeline
{
    [Stage]
    public class DeployToTesting
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_nested_stage_nested_job()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_nested_stage_nested_job) + @""")]
public class Pipeline
{
    [Stage]
    public class DeployToTesting
    {
        [Job]
        public class Deployment
        {
        }
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_nested_stage_nested_job_step()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_nested_stage_nested_job_step) + @""")]
public class Pipeline
{
    [Stage]
    public class DeployToTesting
    {
        [Job]
        public class Deployment
        {
            [Step]
            public void Default()
            {
            }
        }
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_automatron_task_parameters()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using ParameterAttribute = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_automatron_task_parameters) + @""")]
public class Pipeline
{
    [Parameter(Default = false, Values = new object[] { true, false })]
    public bool RunPerfTests { get; set; }

    [Stage]
    [Job]
    [Task(Parameters = new[] { nameof(RunPerfTests) })]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, new Dictionary<string, string>
        {
            {"build_property.AzureDevOpsPipelineProjectDirectory", ProjectPath}
        }, generator);

        // Assert
        generator.Pipelines!.First().Stages.First().Jobs.First().Steps.First().As<AutomatronScript>().Content
            .Should()
            .Contain("--runperftests \"${{ parameters.RunPerfTests }}\"");
    }

    [Fact]
    public void Should_be_able_to_map_boolean_pipeline_parameter()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using ParameterAttribute = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_boolean_pipeline_parameter) + @""")]
public class Pipeline
{
    [Parameter(Default = false, Values = new object[] { true, false })]
    public bool RunPerfTests { get; set; }

    [Stage]
    [Job]
    [Task(Parameters = new[] { nameof(RunPerfTests) })]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, new Dictionary<string, string>
        {
            {"build_property.AzureDevOpsPipelineProjectDirectory", ProjectPath}
        }, generator);

        // Assert
        generator.Pipelines!.First().Parameters
            .Should()
            .Satisfy(c =>
                c.Name == "RunPerfTests" &&
                c.Type == ParameterTypes.Boolean &&
                (bool)c.Value! == false &&
                c.Values!.Length == 2
            );
    }

    [Fact]
    public void Should_be_able_to_map_multiple_pipeline_parameters()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using ParameterAttribute = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

[Pipeline(YmlName = """+nameof(Should_be_able_to_map_multiple_pipeline_parameters)+ @""")]
public class Pipeline
{
    [Parameter]
    public bool RunPerfTests1 { get; set; }

    [Parameter]
    public bool RunPerfTests2 { get; set; }

    [Stage]
    [Job]
    [Task]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation,new Dictionary<string, string>
        {
            {"build_property.AzureDevOpsPipelineProjectDirectory", ProjectPath}
        }, generator);

        // Assert
        generator.Pipelines!.First().Parameters
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_variable()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_variable) + @""")]
[Variable(""NUGET.PLUGIN.HANDSHAKE.TIMEOUT.IN.SECONDS"", 60)]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Variables.Should().NotBeNull().And.ContainSingle()
            .Subject.Should().BeOfType<Variable>()
            .Subject.Value.Should().Be(60);
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_stage_job_variable_env()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_stage_job_variable_env) + @""")]
[Stage]
[Job]
public class Pipeline
{
    [Variable]
    public string? Var1 { get; set; }

    [Step]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle()
            .Subject.Should().BeOfType<AutomatronScript>()
            .Subject.Env.Should().ContainKey("VAR1");
    }

    [Fact]
    public void Should_be_able_to_map_pipeline_nested_stage_nested_job_variable_env()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_pipeline_nested_stage_nested_job_variable_env) + @""")]
public class Pipeline
{
    [Stage]
    public class DeployToTesting
    {
        [Job]
        public class Deployment
        {
            [Variable]
            public string? Var1 { get; set; }

            [Step]
            public void Default()
            {
            }
        }
    }

    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle()
            .Subject.Should().BeOfType<AutomatronScript>()
            .Subject.Env.Should().ContainKey("VAR1");
    }

    [Fact]
    public void Should_be_able_to_map_variables_group()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_variables_group) + @""")]
[VariableGroup(""nuget"")]
public class Pipeline
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.First().Variables.Should().HaveCount(1);
        generator.Pipelines.First().Variables!.First().As<VariableGroup>().Name.Should().Be("nuget");
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline) + @""")]
public abstract class PipelineBase
{
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull()
            .And.Subject.Should().ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_stage()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_stage) + @""")]
public abstract class PipelineBase
{
    [Stage]
    public class DeployToTesting
    {
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_inherited_stage()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Stage]
public abstract class Deployment
{
}

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_inherited_stage) + @""")]
public abstract class PipelineBase
{
    public class DeployToTesting : Deployment
    {
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_inherited_stage_nested_inherited_job()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Job]
public abstract class JobBase
{
}

[Stage]
public abstract class Deployment
{
    [Job]
    public class Deploy : JobBase
    {
    }
}

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_inherited_stage_nested_inherited_job) + @""")]
public abstract class PipelineBase
{
    public class DeployToTesting : Deployment
    {
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_inherited_stage_nested_inherited_job_step()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Job]
public abstract class JobBase
{
    [Step]
    public void Default()
    {
    }
}

[Stage]
public abstract class Deployment
{
    [Job]
    public class Deploy : JobBase
    {
    }
}

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_inherited_stage_nested_inherited_job_step) + @""")]
public abstract class PipelineBase
{
    public class DeployToTesting : Deployment
    {
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_stage_nested_job()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_stage_nested_job) + @""")]
public abstract class PipelineBase
{
    [Stage]
    public class DeployToTesting
    {
       [Job]
        public class Deployment
        {
        }
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle();
    }

    [Fact]
    public void Should_be_able_to_map_inherited_pipeline_nested_stage_nested_job_step()
    {
        // Arrange
        const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Annotations;

[Pipeline(YmlName = """ + nameof(Should_be_able_to_map_inherited_pipeline_nested_stage_nested_job_step) + @""")]
public abstract class PipelineBase
{
    [Stage]
    public class DeployToTesting
    {
        [Job]
        public class Deployment
        {
            [Step]
            public void Default()
            {
            }
        }
    }
}

public class Pipeline :PipelineBase
{
    public static void Main(string[] args)
    {
    }
}
";
        var generator = new PipelineYamlGenerator();
        var compilation = CreateCompilation(code, "Pipeline");

        // Act
        RunGenerator(compilation, _options, generator);

        // Assert
        generator.Pipelines.Should().NotBeNull().And.ContainSingle()
            .Subject.Stages.Should().NotBeNull().And.ContainSingle()
            .Subject.Jobs.Should().NotBeNull().And.ContainSingle()
            .Subject.Steps.Should().NotBeNull().And.ContainSingle();
    }


    private static Compilation CreateCompilation(string source,string mainTypeName)
    {
        var list = new List<MetadataReference>();

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        list.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        list.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "mscorlib.dll")));
        list.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "System.dll")));
        list.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "System.Core.dll")));
        list.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "System.Runtime.dll")));
        list.Add(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location));
        list.Add(MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location));

        list.Add(MetadataReference.CreateFromFile(typeof(Pipeline).Assembly.Location));
        list.Add(MetadataReference.CreateFromFile(typeof(Secret).Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "compilation",
            syntaxTrees: new[]
                { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.LatestMajor)) },

            references: list,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication).WithMainTypeName(mainTypeName)
        );
    }

    private static GeneratorDriver CreateDriver(Compilation compilation, AnalyzerConfigOptionsProvider? optionsProvider, params ISourceGenerator[] generators)
    {
        return CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generators),
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            optionsProvider: optionsProvider
        );
    }

    private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(Compilation compilation, Dictionary<string, string> globalOptions, ISourceGenerator generator)
    {
        var optionsProvider = Substitute.For<AnalyzerConfigOptionsProvider>();

        foreach (var globalOption in globalOptions)
        {
            optionsProvider.GlobalOptions.TryGetValue(globalOption.Key, out Arg.Any<string>()!).Returns(x => {
                x[1] = globalOption.Value;
                return true;
            });
        }

        CreateDriver(compilation, optionsProvider, generator).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);
        return (updatedCompilation, diagnostics);
    }
}