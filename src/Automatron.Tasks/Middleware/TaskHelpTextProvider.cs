#if NET8_0
using System.Collections.Generic;
using System.Text;
using Automatron.Tasks.Commands;
using Automatron.Tasks.Models;
using CommandDotNet;
using CommandDotNet.Help;

namespace Automatron.Tasks.Middleware;

internal class TaskHelpProvider : HelpTextProvider
{
    private readonly string _definitionSource = typeof(TaskCommand).FullName + "." + nameof(TaskCommand.Run);

    private readonly IHelpProvider _inner;
    private readonly IEnumerable<Task> _tasks;
    private readonly IEnumerable<Parameter> _parameters;

    public TaskHelpProvider(IHelpProvider inner, IEnumerable<Task> tasks, IEnumerable<Parameter> parameters, AppSettings appSettings) : base(appSettings)
    {
        _inner = inner;
        _tasks = tasks;
        _parameters = parameters;
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
                ("Tasks", SectionTasks(command)),
                ("Parameters", SectionParameters()),
                (null, ExtendedHelpText(command)));
        }

        return _inner.GetHelpText(command);
    }

    protected string SectionParameters()
    {
        var sb = new StringBuilder();

        foreach (var parameter in _parameters)
        {
            sb.Append($"{PadFront(parameter.Name)}");
            if (parameter.Value != null)
            {
                sb.Append($" ({parameter.Value})");
            }
            sb.AppendLine();
            if (!string.IsNullOrEmpty(parameter.Description))
            {
                sb.AppendLine(PadFront(parameter.Description));
            }
        }

        return sb.ToString();
    }

    protected virtual string SectionTasks(Command command)
    {
        var sb = new StringBuilder();

        foreach (var task in _tasks)
        {
            sb.AppendLine(PadFront(task.Name));
        }

        return sb.ToString();
    }
}

#endif