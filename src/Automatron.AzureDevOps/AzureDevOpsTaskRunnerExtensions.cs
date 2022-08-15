#if NET6_0
using System;
using CommandDotNet;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Automatron.AzureDevOps;

public static class AzureDevOpsTaskRunnerExtensions
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services)
    {
        return services.AddSingleton<AzureDevOpsTasks>().AddSingleton<ITaskModelFactory>(c=> new AzureDevOpsTaskModelFactoryDecorator(
            c.GetRequiredService<TaskModelFactory>(),
            c.GetRequiredService<IServiceProvider>(),
            c.GetRequiredService<ITypeProvider>()
            ));
    }

    public static TaskRunner UseAzureDevOps(this TaskRunner taskRunner)
    {
        return taskRunner.ConfigureServices(c =>
        {
            c.AddSingleton(provider =>
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
            c.AddAzureDevOps();
        });
    }
}
#endif