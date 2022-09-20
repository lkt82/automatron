﻿#if NET6_0
using System.Collections.Generic;
using System.Text;
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Models;
using CommandDotNet;
using CommandDotNet.Help;

namespace Automatron.AzureDevOps.Middleware;

internal class PipelineHelpProvider : HelpTextProvider
{
    private readonly string _definitionSource = typeof(AzureDevOpsCommand).FullName + "." + nameof(AzureDevOpsCommand.Run);

    private readonly IHelpProvider _inner;
    private readonly IEnumerable<Pipeline> _pipelines;

    public PipelineHelpProvider(IHelpProvider inner, IEnumerable<Pipeline> pipelines, AppSettings appSettings) : base(appSettings)
    {
        _inner = inner;
        _pipelines = pipelines;
    }

    public override string GetHelpText(Command command)
    {
        if (command.DefinitionSource == _definitionSource)
        {
            return JoinSections(
                (null, CommandDescription(command)),
                (Resources.A.Help_Usage, SectionUsage(command)),
                (Resources.A.Help_Commands, SectionSubcommands(command)),
                (Resources.A.Help_Options, SectionOptions(command, false)),
                ("Pipelines", SectionPipelines()),
                ("Variables", SectionVariables()),
                (null, ExtendedHelpText(command)));
        }

        return _inner.GetHelpText(command);
    }

    protected string SectionVariables()
    {
        var sb = new StringBuilder();

        foreach (var pipeline in _pipelines)
        {
            foreach (var variable in pipeline.Variables)
            {
                sb.Append($"{PadFront(pipeline.Name)} -> {variable.Name}");
                if (variable.Value != null)
                {
                    sb.Append($" ({variable.Value})");
                }
                sb.AppendLine();
                if (!string.IsNullOrEmpty(variable.Description))
                {
                    sb.AppendLine(PadFront(variable.Description));
                }
            }
        }

        return sb.ToString();
    }

    protected virtual string SectionPipelines()
    {
        var sb = new StringBuilder();

        foreach (var pipeline in _pipelines)
        {
            sb.AppendLine(PadFront(pipeline.Name));
        }

        return sb.ToString();
    }
}

#endif