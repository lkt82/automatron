using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Automatron.Annotations;
using Bullseye;
using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Execution;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TypeInfo = CommandDotNet.TypeInfo;

namespace Automatron
{
    public class TaskRunner<TController> where TController : class
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

        private IEnumerable<(Type, IEnumerable<MethodInfo>)> GetTargets(IEnumerable<Type> types)
        {
            foreach (var controllerType in types)
            {
                yield return (controllerType, controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod).Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName));

                var interfaces = controllerType.GetInterfaces().ToArray();

                if (!interfaces.Any())
                {
                    continue;
                }

                foreach (var targets in GetTargets(interfaces))
                {
                    yield return (controllerType, targets.Item2);
                }
            }
        }

        private IEnumerable<(Type, IEnumerable<PropertyInfo>)> GetOptions(IEnumerable<Type> types)
        {
            foreach (var controllerType in types)
            {
                yield return (controllerType, controllerType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName && c.CanWrite));

                var interfaces = controllerType.GetInterfaces().ToArray();

                if (!interfaces.Any())
                {
                    continue;
                }

                foreach (var options in GetOptions(interfaces))
                {
                    yield return (controllerType, options.Item2);
                }
            }
        }



        private IEnumerable<PropertyInfo> ControllerOptions => GetOptions(_controllerTypes).SelectMany(c=> c.Item2);

        private IEnumerable<(Type, IEnumerable<MethodInfo>)> ControllerTargets => GetTargets(_controllerTypes);

        private void AddControllerOptions(BuildEvents.CommandCreatedEventArgs args)
        {
            foreach (var property in ControllerOptions)
            {
                if (args.CommandBuilder.Command.ContainsArgumentNode(property.Name))
                {
                    continue;
                }

                var optionAttribute = property.GetCustomAttribute<OptionAttribute>();


                TypeInfo typeInfo;
                IArgumentArity arity;

                if (property.PropertyType.IsArray)
                {
                    typeInfo = new TypeInfo(property.PropertyType, property.PropertyType.GetElementType()!);
                    arity = ArgumentArity.OneOrMore;
                }
                else if (property.PropertyType.IsGenericType && (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) || property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    typeInfo = new TypeInfo(property.PropertyType, property.PropertyType.GetGenericArguments().First());
                    arity = ArgumentArity.OneOrMore;
                }
                else
                {
                    typeInfo = new TypeInfo(property.PropertyType, property.PropertyType);
                    arity = ArgumentArity.ExactlyOne;
                }

                #pragma warning disable CS0618
                var option = new Option(optionAttribute?.LongName??char.ToLowerInvariant(property.Name[0]) + property.Name[1..], optionAttribute?.ShortName, typeInfo, arity, optionAttribute?.BooleanMode??BooleanMode.Explicit, typeof(TController).FullName,customAttributes:new TaskAttributeProvider(property))
                {
                    Description = optionAttribute?.Description,
                    Split = optionAttribute?.Split,
                    IsMiddlewareOption = true,
                    Hidden = false
                };
                #pragma warning restore CS0618

                _optionLockUp[property] = option;

                args.CommandBuilder.AddArgument(option);
            }
        }

        private Task<int> CreateController(CommandContext ctx, ExecutionDelegate next)
        {
            var controller = ctx.DependencyResolver!.Resolve<TController>();

            foreach (var property in ControllerOptions)
            {
                var option = _optionLockUp[property];

                if (option.Value == null && option.Default?.Value == null)
                {
                    continue;
                }

                property.SetValue(controller, option.Value?? option.Default?.Value);
            }

            return next(ctx);
        }

        private Task<int> BuildBullseyeTargets(CommandContext ctx, ExecutionDelegate next)
        {
            var targets = ControllerTargets.SelectMany(target =>
            {
                return target.Item2.Select(methodInfo =>
                {
                    var dependentOnAttribute = methodInfo.GetCustomAttribute<DependentOnAttribute>();
                    var dependentForAttribute = methodInfo.GetCustomAttribute<DependentForAttribute>();

                    var dependentOn = Enumerable.Empty<string>();

                    var serviceType = target.Item1;

                    if (dependentOnAttribute != null)
                    {
                        if (methodInfo.ReflectedType == ControllerType)
                        {
                            dependentOn = dependentOnAttribute?.Targets;
                        }
                        else if (dependentOnAttribute.Controller is { IsClass: true })
                        {
                            dependentOn = dependentOnAttribute.Targets.Select(t => dependentOnAttribute.Controller.Name + t);
                        }
                        else
                        {
                            dependentOn = dependentOnAttribute.Targets.Select(t => target.Item1.Name + t);
                        }
                    }

                    return new
                    {
                        Name = target.Item1 != ControllerType
                            ? target.Item1.Name + methodInfo.Name
                            : methodInfo.Name,
                        DependentOn = dependentOn ?? Enumerable.Empty<string>(),
                        DependentFor = dependentForAttribute?.Targets ?? Enumerable.Empty<string>(),
                        Method = methodInfo,
                        Service = serviceType!
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
            var appRunner = new AppRunner<BullseyeCommand>();

            Services.AddSingleton(_bullseyeTargets);
            Services.AddSingleton(typeof(BullseyeCommand));

            foreach (var type in _controllerTypes)
            {
                Services.AddSingleton(type);
            }
            
            return appRunner
                .Configure(c =>
                {
                    Services.AddSingleton(c.Console);
                    c.UseMiddleware(CreateController, MiddlewareStages.PostBindValuesPreInvoke);
                    c.UseMiddleware(BuildBullseyeTargets, MiddlewareStages.PostBindValuesPreInvoke);
                    c.BuildEvents.OnCommandCreated += AddControllerOptions;
                })
                .UseErrorHandler((_, _) => ExitCodes.Error.Result)
                .UseDefaultsFromEnvVar()
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
}

