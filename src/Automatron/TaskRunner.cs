using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Automatron.Annotations;
using Bullseye;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Execution;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TypeInfo = CommandDotNet.TypeInfo;

namespace Automatron;

public sealed class TaskRunner<TController> where TController : class
{
    internal class TaskAttributeProvider : ICustomAttributeProvider
    {
        private readonly PropertyInfo _propertyInfo;

        public TaskAttributeProvider(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public object[] GetCustomAttributes(bool inherit)
        {
            return _propertyInfo.GetCustomAttributes(inherit);
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _propertyInfo.GetCustomAttributes(attributeType,inherit);
        }

        public bool IsDefined(Type attributeType, bool inherit)
        {
            return _propertyInfo.IsDefined(attributeType, inherit);
        }
    }

    private readonly Targets _bullseyeTargets = new();
    private readonly object _objectType = typeof(object);
    private static readonly Type ControllerType = typeof(TController);

    private readonly IEnumerable<Type> _controllerTypes = GetControllerTypes().ToArray();

    private readonly IDictionary<PropertyInfo, Option> _optionLockUp = new Dictionary<PropertyInfo, Option>();

    private static IEnumerable<Type> GetControllerTypes()
    {
        return GetControllerTypes(ControllerType).Concat(ControllerType.Assembly.GetTypes().Where(c => c.GetCustomAttribute<TaskControllerAttribute>() != null));
    }

    private static IEnumerable<Type> GetControllerTypes(Type type)
    {
        var types = new HashSet<Type>();

        types.UnionWith(type.GetNestedTypes().Where(c => !c.IsAbstract));
        types.UnionWith(type.GetInterfaces().SelectMany(c=> c.GetNestedTypes()).Where(c => !c.IsAbstract));

        types.UnionWith(types.SelectMany(GetControllerTypes));

        types.Add(type);

        return types;
    }

    private IEnumerable<(Type, IEnumerable<MethodInfo>)> GetTasks(IEnumerable<Type> types)
    {
        foreach (var controllerType in types)
        {
            yield return (controllerType, controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod).Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName));

            var interfaces = controllerType.GetInterfaces().ToArray();

            if (!interfaces.Any())
            {
                continue;
            }

            foreach (var targets in GetTasks(interfaces))
            {
                yield return (controllerType, targets.Item2);
            }
        }
    }

    private IEnumerable<(Type, IEnumerable<PropertyInfo>)> GetParameters(IEnumerable<Type> types)
    {
        foreach (var controllerType in types)
        {
            yield return (controllerType, controllerType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName && c.CanWrite));

            var interfaces = controllerType.GetInterfaces().ToArray();

            if (!interfaces.Any())
            {
                continue;
            }

            foreach (var options in GetParameters(interfaces))
            {
                yield return (controllerType, options.Item2);
            }
        }
    }

    private IEnumerable<(Type, IEnumerable<PropertyInfo>)> ControllerParameters => GetParameters(_controllerTypes).ToArray();

    private IEnumerable<(Type, IEnumerable<MethodInfo>)> ControllerTasks => GetTasks(_controllerTypes).ToArray();

    private void AddControllerParameters(BuildEvents.CommandCreatedEventArgs args)
    {
        foreach (var (_, properties) in ControllerParameters)
        {
            foreach (var parameter in properties)
            {
                if (args.CommandBuilder.Command.ContainsArgumentNode(parameter.Name))
                {
                    continue;
                }

                TypeInfo typeInfo;
                IArgumentArity arity;

                if (parameter.PropertyType.IsArray)
                {
                    typeInfo = new TypeInfo(parameter.PropertyType, parameter.PropertyType.GetElementType()!);
                    arity = ArgumentArity.OneOrMore;
                }
                else if (parameter.PropertyType.IsGenericType &&
                         (parameter.PropertyType.GetGenericTypeDefinition() == typeof(List<>) ||
                          parameter.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    typeInfo = new TypeInfo(parameter.PropertyType,
                        parameter.PropertyType.GetGenericArguments().First());
                    arity = ArgumentArity.OneOrMore;
                }
                else
                {
                    typeInfo = new TypeInfo(parameter.PropertyType, parameter.PropertyType);
                    arity = ArgumentArity.ExactlyOne;
                }

                var name = parameter.Name.ToLower();

                var descriptionAttribute = parameter.GetCustomAttribute<DescriptionAttribute>();

                var option = new Option(name, null, typeInfo, arity, BooleanMode.Explicit, parameter.Name,
                    customAttributes: new TaskAttributeProvider(parameter))
                {
                    Description = descriptionAttribute?.Description,
                    Split = arity.AllowsOneOrMore() ? ',' : null,
                    IsMiddlewareOption = true,
                    Default = GetParameterDefault(args.CommandContext.Environment, parameter.Name, typeInfo),
                    Hidden = false
                };

                _optionLockUp[parameter] = option;

                args.CommandBuilder.AddArgument(option);
            }
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

    private Task<int> CreateController(CommandContext ctx, ExecutionDelegate next)
    {
        foreach (var (type, properties) in ControllerParameters)
        {
            foreach (var property in properties)
            {
                var controller = ctx.DependencyResolver!.Resolve(type);

                var option = _optionLockUp[property];

                if (option.Value == null && option.Default?.Value == null)
                {
                    continue;
                }

                property.SetValue(controller, option.Value ?? option.Default?.Value);
            }
        }

        return next(ctx);
    }

    private Task<int> BuildTasks(CommandContext ctx, ExecutionDelegate next)
    {
        var targets = ControllerTasks.SelectMany(target =>
        {
            var (serviceType, methods) = target;
            return methods.Select(method =>
            {
                var dependentOnAttribute = method.GetCustomAttribute<DependentOnAttribute>();
                var dependentForAttribute = method.GetCustomAttribute<DependentForAttribute>();

                var dependentOn = Enumerable.Empty<string>();
                var dependentFor = Enumerable.Empty<string>();

                if (dependentOnAttribute != null)
                {
                    if (dependentOnAttribute.Controller is { IsClass: true })
                    {
                        dependentOn = dependentOnAttribute.Targets.Select(t => dependentOnAttribute.Controller.Name + t);
                    }
                    else if (method.ReflectedType == ControllerType)
                    {
                        dependentOn = dependentOnAttribute.Targets;
                    }
                    else
                    {
                        dependentOn = dependentOnAttribute.Targets.Select(t => serviceType.Name + t);
                    }
                }

                if (dependentForAttribute != null)
                {
                    if (dependentForAttribute.Controller is { IsClass: true })
                    {
                        dependentFor = dependentForAttribute.Targets.Select(t => dependentForAttribute.Controller.Name + t);
                    }
                    else if (method.ReflectedType == ControllerType)
                    {
                        dependentFor = dependentForAttribute.Targets;
                    }
                    else
                    {
                        dependentFor = dependentForAttribute.Targets.Select(t => serviceType.Name + t);
                    }
                }


                return new
                {
                    Name = serviceType != ControllerType
                        ? serviceType.Name + method.Name
                        : method.Name,
                    DependentOn = dependentOn,
                    DependentFor = dependentFor,
                    Method = method,
                    Service = serviceType
                };
            });
        }).ToArray();

        var taskType = typeof(Task);

        foreach (var target in targets)
        {
            var dependencies = new HashSet<string>();

            dependencies.UnionWith(target.DependentOn);

            var dependentFor = targets.Where(c => c.Name != target.Name && c.DependentFor.Contains(target.Name))
                .Select(c => c.Name);

            dependencies.UnionWith(dependentFor);


            if (taskType.IsAssignableFrom(target.Method.ReturnType))
            {
                _bullseyeTargets.Add(target.Name, dependencies, () => (Task)target.Method.Invoke(ctx.DependencyResolver!.Resolve(target.Service), null)!);
            }
            else
            {
                _bullseyeTargets.Add(target.Name, dependencies, () => target.Method.Invoke(ctx.DependencyResolver!.Resolve(target.Service), null));
            }
        }

        return next(ctx);
    }

    private ServiceCollection Services { get; } = new();

    public TaskRunner<TController> ConfigureServices(Action<ServiceCollection> action)
    {
        action(Services);
        return this;
    }

    private AppRunner Build()
    {
        var appRunner = new AppRunner<TaskCommand>();

        Services.AddSingleton(_bullseyeTargets);
        Services.AddSingleton(typeof(TaskCommand));

        foreach (var type in _controllerTypes)
        {
            Services.AddSingleton(type);
        }

        return appRunner
            .Configure(c =>
            {
                Services.AddSingleton(c.Console);
                Services.AddSingleton(c.Environment);
                c.UseMiddleware(CreateController, MiddlewareStages.PostBindValuesPreInvoke);
                c.UseMiddleware(BuildTasks, MiddlewareStages.PostBindValuesPreInvoke);
                c.BuildEvents.OnCommandCreated += AddControllerParameters;
            })
            .UseErrorHandler((_, _) => ExitCodes.Error.Result)
            .UseCancellationHandlers()
            .UseMicrosoftDependencyInjection(Services.BuildServiceProvider());
    }

    public async Task<int> RunAsync(params string[] args)
    {
        return await Build().RunAsync(args);
    }

    public int Run(params string[] args)
    {
        return Build().Run(args);
    }

}