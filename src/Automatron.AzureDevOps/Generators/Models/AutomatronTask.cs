using System.Text;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class AutomatronTask : Script
{
    public AutomatronTask(IJob job, string[] targets, bool skipDependencies = false, bool parallel = false, string[]? parameters = null) : base(job, BuildCommand(targets, skipDependencies, parallel, parameters))
    {
    }

    private static string BuildCommand(string[] targets, bool skipDependencies, bool parallel, string[]? parameters = null)
    {
        var arguments = new StringBuilder();

        arguments.Append("dotnet run -- -r ");

        arguments.Append(string.Join(" ", targets));

        if (skipDependencies)
        {
            arguments.Append(" -s");
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

        return arguments.ToString();
    }
}