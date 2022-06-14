using System;
using System.Diagnostics;
using System.IO;
using Automatron.AzureDevOps.Generators.Converters;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Automatron.AzureDevOps.Generators;

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
    
        Debug.WriteLine($"Execute {nameof(YamlPipelineGenerator)}");

        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.PipelineProjectDirectory",
                out var projectDirectory))
        {
            throw new InvalidOperationException();
        }

        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

        if (mainMethod == null)
        {
            return;
        }

        var pipelineVisitor = new PipelineVisitor(projectDirectory);
        pipelineVisitor.Visit(mainMethod.ContainingAssembly.GlobalNamespace);

        foreach (var pipeline in pipelineVisitor.Pipelines)
        {
            SavePipeline(pipeline);
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
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
#endif
        Debug.WriteLine($"Initialize {nameof(YamlPipelineGenerator)}");
    }
}