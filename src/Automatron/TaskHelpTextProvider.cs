#if NET6_0
using System;
using System.Linq;
using CommandDotNet;
using CommandDotNet.Help;

namespace Automatron;

internal class TaskHelpTextProvider : HelpTextProvider
{
    private readonly TaskModel _taskModel;

    public TaskHelpTextProvider(AppSettings appSettings, TaskModel taskModel) : base(appSettings)
    {
        _taskModel = taskModel;
    }

    protected override string? UsageOption(Command command) =>
        command.Options.Any(o => !o.Hidden)
            ? "[parameters]"
            : null;

    public override string GetHelpText(Command command) =>
        JoinSections(
            (null, CommandDescription(command)),
            (Resources.A.Help_Usage, SectionUsage(command)),
            (Resources.A.Help_Arguments, SectionOperands(command)),
            ("Tasks", string.Join(Environment.NewLine, _taskModel.Tasks.Select(c=> c.Name))),
            ("Parameters", SectionOptions(command, false)),
            (null, ExtendedHelpText(command)));

}
#endif