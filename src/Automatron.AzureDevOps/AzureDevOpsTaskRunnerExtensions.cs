#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.Reflection;
using CommandDotNet;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using ParameterAttribute = Automatron.Annotations.ParameterAttribute;

namespace Automatron.AzureDevOps;

public static class AzureDevOpsTaskRunnerExtensions
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var environment = provider.GetRequiredService<IEnvironment>();
            if (environment.GetEnvironmentVariable("TF_BUILD") != "True")
            {
                return AnsiConsole.Console;
            }
            var ansiConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Yes,
                ColorSystem = ColorSystemSupport.Standard,
                Interactive = InteractionSupport.No,

                Out = new AnsiConsoleOutput(Console.Out)
            });
            ansiConsole.Profile.Width = 1000;
            return ansiConsole;

        });

        return services
            .AddSingleton<AzureDevOpsTasks>()
            .AddTransient<TaskVisitor, PipelineTaskVisitor>();
    }

    private static IEnumerable<Type> GetAssemblyTypes()
    {
        return Assembly.GetEntryAssembly()!.GetTypes()
            .Where(c => !c.IsAbstract && !c.IsInterface && c.IsVisible)
            .Where(c =>
            {
                var isTypePipeline = c.GetCachedAttribute<PipelineAttribute>() != null;
                var isTypeStage = c.GetCachedAttribute<StageAttribute>() != null;
                var isTypeJob = c.GetCachedAttribute<JobAttribute>() != null;
                var hasMethodSteps = c.GetMethods().Any(m => m.GetCachedAttribute<StepAttribute>() != null);
                var hasInterfacesMethodSteps = c.GetInterfaces().SelectMany(t => t.GetMethods()).Any(m => m.GetCachedAttribute<StepAttribute>() != null);
                var hasVariables = c.GetProperties().Any(m => m.GetCachedAttribute<VariableAttribute>() != null);
                var hasInterfacesVariables = c.GetInterfaces().SelectMany(t => t.GetProperties()).Any(m => m.GetCachedAttribute<ParameterAttribute>() != null);

                return isTypePipeline || isTypeStage || isTypeJob || hasMethodSteps || hasVariables || hasInterfacesMethodSteps || hasVariables || hasInterfacesVariables;
            });
    }

    public static TaskRunner UseAzureDevOps(this TaskRunner taskRunner)
    {
        return taskRunner
            .UsingTypes(GetAssemblyTypes())
            .ConfigureServices(c => c.AddAzureDevOps());
    }
}
#endif