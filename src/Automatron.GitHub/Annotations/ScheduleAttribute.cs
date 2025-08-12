using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class ScheduleAttribute(string cron) : Attribute
{
    public string Cron { get; } = cron;
}