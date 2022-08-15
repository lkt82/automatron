#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Automatron.Annotations;
using Automatron.Reflection;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Execution;
using CommandDotNet.Extensions;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.Spectre;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TypeInfo = CommandDotNet.TypeInfo;

namespace Automatron;

public sealed class TaskRunner : ITypeProvider
{
    private readonly IDictionary<Parameter, Option> _optionLockUp = new Dictionary<Parameter, Option>();

    private readonly IEnumerable<Type> _types = GetAssemblyTypes().ToArray();

    public TaskRunner()
    {
        Services.AddSingleton(typeof(TaskCommand));
        Services.AddSingleton<ITypeProvider>(this);
 
        foreach (var type in _types)
        {
            Services.AddScoped(type);
        }

        Services.AddSingleton(_ => AnsiConsole.Console);
        Services.AddSingleton<IConsole, AnsiConsoleForwardingConsole>();
        Services.AddSingleton<IEnvironment>(_ => new SystemEnvironment());

        Services.AddSingleton<IActionRunner, ActionRunner>();
        Services.AddSingleton<TaskVisitor>();
        Services.AddSingleton<TaskModelFactory>();
        Services.AddSingleton<ITaskModelFactory, TaskModelFactory>();
        Services.AddSingleton<ITaskEngine, TaskEngine>();
        Services.AddSingleton(c => c.GetRequiredService<ITaskModelFactory>().Create());
    }

    private static IEnumerable<Type> GetAssemblyTypes()
    {
        return Assembly.GetEntryAssembly()!.GetTypes()
            .Where(c=> !c.IsAbstract && !c.IsInterface && c.IsVisible)
            .Where(c =>
            {
                var isTypeTask = c.GetCachedAttribute<TaskAttribute>() != null;
                var hasMethodTasks = c.GetMethods().Any(m => m.GetCachedAttribute<TaskAttribute>() != null);
                var hasInterfacesMethodTasks = c.GetInterfaces().SelectMany(t=> t.GetMethods()).Any(m => m.GetCachedAttribute<TaskAttribute>() != null);
                var hasParameters = c.GetProperties().Any(m => m.GetCachedAttribute<ParameterAttribute>() != null);
                var hasInterfacesParameters = c.GetInterfaces().SelectMany(t => t.GetProperties()).Any(m => m.GetCachedAttribute<ParameterAttribute>() != null);

                return isTypeTask || hasMethodTasks || hasParameters || hasInterfacesMethodTasks || hasParameters|| hasInterfacesParameters;
            });
    }

    private void AddParameters(BuildEvents.CommandCreatedEventArgs args)
    {
        var taskModel = args.CommandContext.DependencyResolver!.Resolve<TaskModel>()!;

        foreach (var parameter in taskModel.Parameters)
        {
            if (args.CommandBuilder.Command.ContainsArgumentNode(parameter.Name))
            {
                continue;
            }

            TypeInfo typeInfo;
            IArgumentArity arity;

            if (parameter.Type.IsArray)
            {
                typeInfo = new TypeInfo(parameter.Type, parameter.Type.GetElementType()!);
                arity = ArgumentArity.OneOrMore;
            }
            else if (parameter.Type.IsGenericType &&
                     (parameter.Type.GetGenericTypeDefinition() == typeof(List<>) ||
                      parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                typeInfo = new TypeInfo(parameter.Type, parameter.Type.GetGenericArguments().First());
                arity = ArgumentArity.OneOrMore;
            }
            else
            {
                typeInfo = new TypeInfo(parameter.Type, parameter.Type);
                arity = ArgumentArity.ExactlyOne;
            }

            var option = new Option(parameter.Name, null, typeInfo, arity, BooleanMode.Explicit, parameter.Name,
                aliases: new[] { parameter.Name.ToLower() }
                )
            {
                Split = arity.AllowsOneOrMore() ? ',' : null,
                IsMiddlewareOption = true,
                Default = GetParameterDefault(args.CommandContext.Environment, parameter.Name, typeInfo),
                Hidden = false,
                Description = parameter.Description
            };

            _optionLockUp[parameter] = option;

            args.CommandBuilder.AddArgument(option);
        }
    }

    private static ArgumentDefault? GetParameterDefault(IEnvironment environment, string name, ITypeInfo typeInfo)
    {
        var envVarName = GetEnvVarName(name);
        var defaultValue = environment.GetEnvironmentVariable(envVarName);
        return defaultValue != null ? new ArgumentDefault("EnvVar", name, typeInfo.Type == typeof(Secret) ? new Secret(defaultValue) : defaultValue) : null;
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

    private Task<int> MapParameters(CommandContext ctx, ExecutionDelegate next)
    {
        var taskModel = ctx.DependencyResolver!.Resolve<TaskModel>()!;

        foreach (var parameter in taskModel.Parameters)
        {
            var option = _optionLockUp[parameter];

            parameter.Value = option.Value ?? option.Default?.Value;
        }

        return next(ctx);
    }

    private ServiceCollection Services { get; } = new();

    [UsedImplicitly]
    public TaskRunner ConfigureServices(Action<ServiceCollection> action)
    {
        action(Services);
        return this;
    }

    private AppRunner Build()
    {
        return new AppRunner<TaskCommand>()
            .UseMicrosoftDependencyInjection(Services.BuildServiceProvider())
            .Configure(c =>
            {
                c.UseMiddleware(MapParameters, MiddlewareStages.PostBindValuesPreInvoke);
                c.BuildEvents.OnCommandCreated += AddParameters;

                var ansiConsole = c.DependencyResolver!.Resolve<IAnsiConsole>()!;
                c.Environment = c.DependencyResolver!.Resolve<IEnvironment>()!;
                c.Console = c.DependencyResolver!.Resolve<IConsole>()!;

                c.UseParameterResolver(_ => ansiConsole);
                c.Services.Add(ansiConsole);

                c.CustomHelpProvider = new TaskHelpTextProvider(c.AppSettings, c.DependencyResolver!.Resolve<TaskModel>()!);
            })
            .UseCancellationHandlers();
    }

    public async Task<int> RunAsync(params string[] args)
    {
        return await Build().RunAsync(args);
    }

    public int Run(params string[] args)
    {
        return Build().Run(args);
    }

    public IEnumerable<Type> GetTypes()
    {
       return _types;
    }
}
#endif