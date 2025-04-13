#if NET8_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Extensions;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.NameCasing;
using CommandDotNet.Spectre;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Automatron;

public class AutomationRunner : AppRunner
{
    public AutomationRunner(Type type) : base(type)
    {
        Services
            .AddSingleton(type)
            .AddSingleton(_ => AnsiConsole.Console)
            .AddSingleton<IConsole, AnsiConsoleForwardingConsole>()
            .AddSingleton<IEnvironment>(_ => new SystemEnvironment())
            .AddSingleton(ConfigureConsole());

        Configure(c =>
            {
                var ansiConsole = c.DependencyResolver!.Resolve<IAnsiConsole>()!;
                c.Environment = c.DependencyResolver!.Resolve<IEnvironment>()!;
                c.Console = c.DependencyResolver!.Resolve<IConsole>()!;
                
                c.UseParameterResolver(_ => ansiConsole);
                c.Services.Add(ansiConsole);
            })
            .UseCancellationHandlers()
            .UseNameCasing(Case.LowerCase, true)
            .UseTypoSuggestions();

        AppSettings.Commands.InheritCommandsFromBaseClasses = true;
    }

    private readonly List<Action<AppConfigBuilder>> _configureActions = new();

    private AppRunner Build()
    {
        base.Configure(delegate(AppConfigBuilder c)
        {
            Services.AddSingleton(c.AppSettings);
        });
        this.UseMicrosoftDependencyInjection(Services.BuildServiceProvider());
        foreach (var action in _configureActions)
        {
            base.Configure(action);
        }

        return this;
    }

    public new AutomationRunner Configure(Action<AppConfigBuilder> configureCallback)
    {
        _configureActions.Add(configureCallback);
        return this;
    }

    public new Task<int> RunAsync(params string[] args)
    {
        return Build().RunAsync(args);
    }

    public new int Run(params string[] args)
    {
        return Build().Run(args);
    }

    private static Func<IServiceProvider, IAnsiConsole> ConfigureConsole()
    {
        return provider =>
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

        };
    }


    private ServiceCollection Services { get; } = new();

    [UsedImplicitly]
    public AutomationRunner ConfigureServices(Action<ServiceCollection> action)
    {
        action(Services);
        return this;
    }
}

public class AutomationRunner<T> : AutomationRunner
{
    public AutomationRunner() :base(typeof(T))
    {
    }
}

#endif