using System.Text;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class AutomatronTask : Script
{
    public AutomatronTask(IJob job, string[] targets, string[]? skip, bool skipAll = false, bool parallel = false, string[]? parameters = null) : base(job, BuildCommand(targets, skip, skipAll, parallel, parameters))
    {
    }

    private static string BuildCommand(string[] targets, string[]? skip, bool skipAll, bool parallel, string[]? parameters = null)
    {
        var arguments = new StringBuilder();

        //arguments.Append("dotnet run -- -r ");
        arguments.Append("dotnet run --");

        if (skipAll)
        {
            arguments.Append(" -a");
        }
        else if (skip is { Length: > 0 })
        {
            arguments.Append(" -s");
            foreach (var target in skip)
            {
                arguments.Append(" ");
                arguments.Append(target);
            }
        }
        if (parallel)
        {
            arguments.Append(" -p");
        }

        if (parameters != null)
        {
            for (var index = 0; index < parameters.Length; index++)
            {
                if (index == 0)
                {
                    arguments.Append(" ");
                }

                var parameter = parameters[index];
                arguments.Append("--");
                arguments.Append(parameter.ToLower());
                arguments.Append(" ");
                arguments.Append("\"${{ parameters.");
                arguments.Append(parameter);
                arguments.Append(" }}\"");
            }
        }

        arguments.Append(" ");
        arguments.Append(string.Join(" ", targets));

        return arguments.ToString();
    }
}