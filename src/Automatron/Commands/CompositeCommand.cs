﻿#if NET6_0
using CommandDotNet;

namespace Automatron.Commands;

[Command(Description = "Composite Automation Command")]
public abstract class CompositeCommand
{
}
#endif