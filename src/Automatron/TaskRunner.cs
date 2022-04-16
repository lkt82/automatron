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
using TypeInfo = CommandDotNet.TypeInfo;

namespace Automatron
{
    public class TaskRunner<T> where T : class, new()
    {
        private readonly Targets _targets = new();
        private readonly object _objectType = typeof(object);

        private T? _tasks;

        private readonly List<Type> _types = CreateTypes();


        private readonly IDictionary<PropertyInfo, Option> _optionLockUp = new Dictionary<PropertyInfo, Option>();

        private static List<Type> CreateTypes()
        {
            var types = new List<Type>();

            var targetType = typeof(T);

            types.Add(targetType);

            types.AddRange(targetType.GetInterfaces());

            return types;
        }

        private IEnumerable<PropertyInfo> OptionProperties => _types
            .SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName && c.CanWrite);

        private IEnumerable<MethodInfo> TaskMethods => _types
            .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod))
            .Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName);

        private void AddTaskOptions(BuildEvents.CommandCreatedEventArgs args)
        {
            foreach (var property in OptionProperties)
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
                var option = new Option(optionAttribute?.LongName??property.Name, optionAttribute?.ShortName, typeInfo, arity, optionAttribute?.BooleanMode??BooleanMode.Explicit, typeof(T).FullName)
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

        private Task<int> CreateTaskObject(CommandContext ctx, ExecutionDelegate next)
        {
            _tasks = new T();

            foreach (var property in OptionProperties)
            {
                var option = _optionLockUp[property];

                if (option.Value == null)
                {
                    continue;
                }

                property.SetValue(_tasks, option.Value);
            }

            return next(ctx);
        }

        private Task<int> BuildTargets(CommandContext ctx, ExecutionDelegate next)
        {
            var targets = TaskMethods
                .Select(c =>
                    new
                    {
                        c.Name,
                        DependentOn = c.GetCustomAttribute<DependsOnAttribute>()?.Targets ?? Enumerable.Empty<string>(),
                        DependentFor = c.GetCustomAttribute<DependentForAttribute>()?.Targets ?? Enumerable.Empty<string>(),
                        Method = c
                    }
                ).ToArray();

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
                    _targets.Add(target.Name, dependencies, () => (Task)target.Method.Invoke(_tasks, null)!);
                }
                else
                {
                    _targets.Add(target.Name, dependencies, () => target.Method.Invoke(_tasks, null));
                }
            }

            return next(ctx);
        }

        public async Task<int> RunAsync(params string[] args)
        {
            return await new AppRunner<BullseyeCommand>()
                .UseDebugDirective()
                .Configure(c =>
                {
                    c.UseParameterResolver(_ => _targets);
                    c.UseMiddleware(CreateTaskObject, MiddlewareStages.BindValues);
                    c.UseMiddleware(BuildTargets, MiddlewareStages.Invoke);
                    c.BuildEvents.OnCommandCreated += AddTaskOptions;
                })
                .UseErrorHandler((_, _) => ExitCodes.Error.Result)
                .RunAsync(args);
        }
    }
}

