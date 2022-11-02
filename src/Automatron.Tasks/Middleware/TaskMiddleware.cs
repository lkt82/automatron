#if NET6_0
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Automatron.Models;
using Automatron.Tasks.Commands;
using Automatron.Tasks.Models;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Help;
using Microsoft.Extensions.DependencyInjection;
using Task = Automatron.Tasks.Models.Task;

namespace Automatron.Tasks.Middleware;

public static class TaskMiddleware
{
    public static IServiceCollection AddTasks(this IServiceCollection services)
    {
        var typeProvider = new TaskTypeProvider();

        foreach (var serviceType in typeProvider.Types)
        {
            services.AddScoped(serviceType);
        }

        return services
            .AddSingleton<ITypeProvider>(typeProvider)
            .AddSingleton<TaskCommand>()
            .AddSingleton<TaskVisitor>()
            .AddSingleton<ITaskEngine, TaskEngine>()
            .AddSingleton(serviceProvider => serviceProvider.GetRequiredService<TaskVisitor>()
                .VisitTypes(typeProvider.Types).OrderBy(c => c.Name).ToArray().AsEnumerable());
    }

    private static string GetEnvVarName(string name)
    {
        var envVarName = new StringBuilder();

        for (var index = 0; index < name.Length; index++)
        {
            var n = name[index];
            if (index > 0 && char.IsLower(name[index - 1]) && char.IsUpper(n))
            {
                envVarName.Append('_');
                envVarName.Append(n);
            }
            else if (char.IsLower(n))
            {
                envVarName.Append(char.ToUpper(n));
            }
            else
            {
                envVarName.Append(n);
            }
        }

        return envVarName.ToString();
    }

    public static AutomationRunner UseTasks(this AutomationRunner automationRunner)
    {
        automationRunner.ConfigureServices(services => AddTasks(services));

        return automationRunner
            .Configure(c =>
            {
                var tasks = c.DependencyResolver!.Resolve<IEnumerable<Task>>()!.ToArray();
                var environment = c.DependencyResolver!.Resolve<IEnvironment>()!;

                var parameters = tasks.SelectMany(task => task.Parameters).SelectMany(parameterType => parameterType.Parameters).Distinct().OrderBy(parameter => parameter.Name).ToArray();

                foreach (var parameter in parameters)
                {
                    var envVarName = GetEnvVarName(parameter.Name);
                    var defaultValue = environment.GetEnvironmentVariable(envVarName);
                    if (defaultValue == null)
                    {
                        continue;
                    }
                    var conv = TypeDescriptor.GetConverter(parameter.Property.PropertyType);
                    parameter.Value = conv.ConvertTo(defaultValue, parameter.Property.PropertyType);
                }
                c.CustomHelpProvider = new TaskHelpProvider(c.CustomHelpProvider ?? new HelpTextProvider(c.AppSettings), c.DependencyResolver!.Resolve<IEnumerable<Task>>()!, parameters, c.AppSettings);
            });

    }
}
#endif