using System;
using System.Collections.Generic;
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

    private readonly Dictionary<string, string> _vscRoot = new();

    private static string GetGitRoot(string workingDirectory)
    {
        using var git = new Process();
        git.StartInfo.FileName = "git";
        git.StartInfo.Arguments = "rev-parse --show-toplevel";
        git.StartInfo.UseShellExecute = false;
        git.StartInfo.RedirectStandardOutput = true;
        git.StartInfo.WorkingDirectory = workingDirectory;
        git.Start();

        var gitRoot = git.StandardOutput.ReadToEnd();
        git.WaitForExit();
        return gitRoot.TrimEnd('\n');
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AzureDevOpsPipelineSkip", out var skipYamlPipeline) && bool.Parse(skipYamlPipeline))
        {
            return;
        }

        Debug.WriteLine($"Execute {nameof(YamlPipelineGenerator)}");

        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AzureDevOpsPipelineProjectDirectory",
                out var projectDirectory))
        {
            throw new InvalidOperationException("AzureDevOpsPipelineProjectDirectory not set");
        }

        string? vscRoot = null;

        if (!_vscRoot.ContainsKey(projectDirectory))
        {
            vscRoot = GetGitRoot(projectDirectory);
            _vscRoot[projectDirectory] = vscRoot;
        }

        if (vscRoot == null)
        {
            throw new NullReferenceException("vscRoot was null");
        }

        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

        if (mainMethod == null)
        {
            return;
        }

        var pipelineVisitor = new PipelineVisitor(vscRoot, PathExtensions.GetUnixPath(projectDirectory));
        pipelineVisitor.Visit(mainMethod.ContainingAssembly.GlobalNamespace);

        foreach (var pipeline in pipelineVisitor.Pipelines)
        {
            //SavePipeline(pipeline);
        }

        Pipelines = pipelineVisitor.Pipelines;
    }

    public List<Pipeline>? Pipelines { get; set; }

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