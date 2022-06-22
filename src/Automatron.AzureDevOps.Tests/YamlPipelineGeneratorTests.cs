using System.Collections.Immutable;
using Automatron.AzureDevOps.Generators;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using Xunit;

namespace Automatron.AzureDevOps.Tests
{
    public class YamlPipelineGeneratorTests
    {

        private readonly string _projectPath = Path.GetFullPath("../../../");

        [Fact]
        public void Should_be_able_to_map_automatron_task_parameters()
        {
            // Arrange
            const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using ParameterAttribute = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

[Pipeline(""" + nameof(Should_be_able_to_map_boolean_pipeline_parameter) + @""")]
public class Pipeline
{
    [Parameter(Default = false, Values = new object[] { true, false })]
    public bool RunPerfTests { get; set; }

    [Stage]
    [Job]
    [AutomatronTask(Parameters = new[] { nameof(RunPerfTests) })]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
            var generator = new YamlPipelineGenerator();
            var compilation = CreateCompilation(code, "Pipeline");

            // Act
            RunGenerator(compilation, new Dictionary<string, string>
            {
                {"build_property.AzureDevOpsPipelineProjectDirectory", _projectPath}
            }, generator);

            // Assert
            generator.Pipelines!.First().Stages.First().Jobs.First().Steps.First().As<AutomatronTask>().Content
                .Should()
                .Be("dotnet run -- -r Default --runperftests \"${{ parameters.RunPerfTests }}\"");
        }

        [Fact]
        public void Should_be_able_to_map_boolean_pipeline_parameter()
        {
            // Arrange
            const string code = @"
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using ParameterAttribute = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

[Pipeline(""" + nameof(Should_be_able_to_map_boolean_pipeline_parameter) + @""")]
public class Pipeline
{
    [Parameter(Default = false, Values = new object[] { true, false })]
    public bool RunPerfTests { get; set; }

    [Stage]
    [Job]
    [AutomatronTask(Parameters = new[] { nameof(RunPerfTests) })]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
            var generator = new YamlPipelineGenerator();
            var compilation = CreateCompilation(code, "Pipeline");

            // Act
            RunGenerator(compilation, new Dictionary<string, string>
            {
                {"build_property.AzureDevOpsPipelineProjectDirectory", _projectPath}
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

[Pipeline("""+nameof(Should_be_able_to_map_multiple_pipeline_parameters)+ @""")]
public class Pipeline
{
    [Parameter]
    public bool RunPerfTests1 { get; set; }

    [Parameter]
    public bool RunPerfTests2 { get; set; }

    [Stage]
    [Job]
    [AutomatronTask]
    public void Default()
    {
    }

    public static void Main(string[] args)
    {
    }
}
";
            var generator = new YamlPipelineGenerator();
            var compilation = CreateCompilation(code, "Pipeline");

            // Act
            RunGenerator(compilation,new Dictionary<string, string>
            {
                {"build_property.AzureDevOpsPipelineProjectDirectory", _projectPath}
            }, generator);

            // Assert
            generator.Pipelines!.First().Parameters
                .Should()
                .HaveCount(2);
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
    
}