#if NET8_0
using System.Collections.Generic;
using System.Text;
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Models;
using CommandDotNet;
using CommandDotNet.Help;

namespace Automatron.AzureDevOps.Middleware;

internal class PipelineHelpProvider(IHelpProvider inner, IEnumerable<Pipeline> pipelines, AppSettings appSettings)
    : HelpTextProvider(appSettings)
{
    private readonly string _definitionSource = typeof(AzureDevOpsCommand).FullName + "." + nameof(AzureDevOpsCommand.Run);

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
                ("Parameters", SectionParameters()),
                (null, ExtendedHelpText(command)));
        }

        return inner.GetHelpText(command);
    }

    protected string SectionParameters()
    {
        var sb = new StringBuilder();

        foreach (var pipeline in pipelines)
        {
            foreach (var parameter in pipeline.Parameters)
            {
                sb.Append($"{PadFront(pipeline.Name)} -> {parameter.Name}");
                if (parameter.Value != null)
                {
                    sb.Append($" ({parameter.Value})");
                }
                sb.AppendLine();
                if (!string.IsNullOrEmpty(parameter.DisplayName))
                {
                    sb.AppendLine(PadFront(parameter.DisplayName));
                }
            }
        }

        return sb.ToString();
    }

    protected string SectionVariables()
    {
        var sb = new StringBuilder();

        foreach (var pipeline in pipelines)
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

        foreach (var pipeline in pipelines)
        {
            sb.AppendLine(PadFront(pipeline.Name));
        }

        return sb.ToString();
    }
}

#endif