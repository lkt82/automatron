#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using CommandDotNet;
using CommandDotNet.Help;

namespace Automatron;

internal class TaskHelpTextProvider : HelpTextProvider
{
    private readonly Dictionary<string, ControllerTask> _tasks;

    public TaskHelpTextProvider(AppSettings appSettings, Dictionary<string, ControllerTask> tasks) : base(appSettings)
    {
        _tasks = tasks;
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
            ("Tasks", string.Join(Environment.NewLine,_tasks.Values.Select(c=> c.Name))),
            ("Parameters", SectionOptions(command, false)),
            (null, ExtendedHelpText(command)));

}
#endif