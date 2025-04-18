﻿#if NET8_0
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Models;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Help;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Automatron.AzureDevOps.Middleware;

public static class AzureDevOpsMiddleware
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services)
    {
        var typeProvider = new PipelineTypeProvider();

        foreach (var serviceType in typeProvider.Types)
        {
            services.AddScoped(serviceType);
        }

        services.TryAddSingleton<ITypeProvider>(typeProvider);

        return services
            .AddSingleton<AzureDevOpsCommand>()
            .AddSingleton<IPipelineEngine, PipelineEngine>()
            .AddSingleton<LoggingCommands>()
            .AddSingleton<PipelineVisitor>()
            .AddSingleton(serviceProvider =>
            {
                var localTypeProvider = serviceProvider.GetRequiredService<ITypeProvider>();
                return serviceProvider.GetRequiredService<PipelineVisitor>().VisitTypes(localTypeProvider.Types).OrderBy(c => c.Name).ToArray().AsEnumerable();
            });
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

    public static AutomationRunner UseAzureDevOps(this AutomationRunner automationRunner)
    {
        automationRunner.ConfigureServices(services => AddAzureDevOps(services));

        return automationRunner
            .Configure(c =>
            {
                var pipelines = c.DependencyResolver!.Resolve<IEnumerable<Pipeline>>()!.ToArray();
                var environment = c.DependencyResolver!.Resolve<IEnvironment>()!;
    
                var variables = new List<Variable>();

                variables.AddRange(pipelines.SelectMany(pipeline => pipeline.Variables));

                variables.AddRange(pipelines.SelectMany(pipeline => pipeline.Stages.SelectMany(stage => stage.Variables)));

                variables.AddRange(pipelines.SelectMany(pipeline => pipeline.Stages.SelectMany(stage => stage.Jobs.SelectMany(job => job.Variables))));

                foreach (var variable in variables)
                {
                    var envVarName = GetEnvVarName(variable.Name);
                    var defaultValue = environment.GetEnvironmentVariable(envVarName);
                    if (defaultValue == null)
                    {
                        continue;
                    }
                    var conv = TypeDescriptor.GetConverter(variable.Property.PropertyType);
                    variable.Value = conv.ConvertTo(defaultValue, variable.Property.PropertyType);
                }

                var parameters = new List<Parameter>();

                parameters.AddRange(pipelines.SelectMany(pipeline => pipeline.Parameters));

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

                c.CustomHelpProvider = new PipelineHelpProvider(c.CustomHelpProvider ?? new HelpTextProvider(c.AppSettings), pipelines, c.AppSettings);
            });
    }
}
#endif