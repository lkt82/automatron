using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations;

public class PulumiTaskAttribute : StepAttribute
{
    public string? Command { get; set; }

    public string? Stack { get; set; }

    public string? Cwd { get; set; }

    public string? Args { get; set; }

    public override Step Create(ISymbol symbol)
    {
        if (Command != null || Stack != null || Cwd != null || Args != null)
        {
            return new PulumiTask(new PulumiTask.PulumiTaskInputs
            {
                Command = Command,
                Stack = Stack,
                Cwd = Cwd,
                Args = Args
            })
            {
                Name = Name,
                DisplayName = DisplayName,
                Condition = Condition
            };
        }

        return new PulumiTask
        {
            Name = Name,
            DisplayName = DisplayName,
            Condition = Condition
        };
    }
}