using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Automatron.AzureDevOps.Generators.Converters;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.AzureDevOps.IO;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Automatron.AzureDevOps.Generators;

[Generator]
internal class PipelineYamlGenerator : ISourceGenerator
{
    private ISerializer? _serializer;

    private readonly Dictionary<string, string?> _vscRoot = new();


    public void Execute(GeneratorExecutionContext context)
    {
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutomatronAzureDevOpsSkip", out var skipYamlPipeline) && bool.Parse(skipYamlPipeline))
        {
            return;
        }

        Debug.WriteLine($"Execute {nameof(PipelineYamlGenerator)}");

        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutomatronAzureDevOpsProjectDirectory",
                out var projectDirectory))
        {
            throw new InvalidOperationException("AutomatronAzureDevOpsProjectDirectory not set");
        }

        if (!_vscRoot.ContainsKey(projectDirectory))
        {
            _vscRoot[projectDirectory] = PathExtensions.GetGitRoot(projectDirectory);
        }

        var vscRoot = _vscRoot[projectDirectory];

        if (vscRoot == null)
        {
            return;
        }

        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

        if (mainMethod == null)
        {
            return;
        }

        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutomatronAzureDevOpsCommand",
                out var command))
        {
            command = "run";
        }

        if (string.IsNullOrEmpty(command))
        {
            command = "run";
        }

        try
        {
            var pipelineVisitor = new PipelineVisitor(vscRoot, PathExtensions.GetUnixPath(projectDirectory), command);
            pipelineVisitor.Visit(mainMethod.ContainingAssembly.GlobalNamespace);

            foreach (var pipeline in pipelineVisitor.Pipelines)
            {
                SavePipeline(pipeline);
            }

            Pipelines = pipelineVisitor.Pipelines;
        }
#pragma warning disable CS0168
        catch (Exception e)
#pragma warning restore CS0168
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AutomatronAzureDevOpsDebug", out var debug) && bool.Parse(debug))
            {
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
                throw;
            }
        }
    }

    public List<Pipeline> Pipelines { get; set; } = new();

    private static ISerializer CreateYamlSerializer()
    {
        var disabledCiTriggerConverter = new DisabledCiTriggerConverter();
        var serializerBuilder = new SerializerBuilder();

        var serializer = serializerBuilder.WithTypeConverter(disabledCiTriggerConverter)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .DisableAliases()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
            .Build();
        return serializer;
    }

    private void SavePipeline(Pipeline pipeline)
    {
        var combined = Path.Combine(pipeline.ProjectDir, pipeline.YmlDir);
        var dir = Path.GetFullPath(combined);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var filePath = Path.Combine(dir, pipeline.YmlName);

        using var stream = File.CreateText(filePath);
        _serializer?.Serialize(stream, pipeline);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        _serializer = CreateYamlSerializer();

#if DEBUG
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
#endif
        Debug.WriteLine($"Initialize {nameof(PipelineYamlGenerator)}");
    }
}