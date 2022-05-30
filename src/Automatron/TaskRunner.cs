﻿using System;
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
    public class TaskRunner<TController> where TController : class, new()
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

        private TController? _controller;

        private readonly List<Type> _types = GetTypes();


        private readonly IDictionary<PropertyInfo, Option> _optionLockUp = new Dictionary<PropertyInfo, Option>();

        private static List<Type> GetTypes()
        {
            var types = new List<Type>();

            var controllerType = typeof(TController);

            types.Add(controllerType);

            types.AddRange(controllerType.GetInterfaces());

            return types;
        }

        private IEnumerable<PropertyInfo> ControllerOptions => _types
            .SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName && c.CanWrite);

        private IEnumerable<MethodInfo> ControllerTargets => _types
            .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod))
            .Where(c => !ReferenceEquals(c.DeclaringType, _objectType) && !c.IsSpecialName);

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
                var option = new Option(optionAttribute?.LongName??property.Name, optionAttribute?.ShortName, typeInfo, arity, optionAttribute?.BooleanMode??BooleanMode.Explicit, typeof(TController).FullName,customAttributes:new TaskAttributeProvider(property))
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
            _controller = new TController();

            foreach (var property in ControllerOptions)
            {
                var option = _optionLockUp[property];

                if (option.Value == null && option.Default?.Value == null)
                {
                    continue;
                }

                property.SetValue(_controller, option.Value?? option.Default?.Value);
            }

            return next(ctx);
        }

        private Task<int> BuildBullseyeTargets(CommandContext ctx, ExecutionDelegate next)
        {
            var targets = ControllerTargets
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
                    _bullseyeTargets.Add(target.Name, dependencies, () => (Task)target.Method.Invoke(_controller, null)!);
                }
                else
                {
                    _bullseyeTargets.Add(target.Name, dependencies, () => target.Method.Invoke(_controller, null));
                }
            }

            return next(ctx);
        }

        public async Task<int> RunAsync(params string[] args)
        {
            return await new AppRunner<BullseyeCommand>()
                .Configure(c =>
                {
                    c.UseParameterResolver(_ => _bullseyeTargets);
                    c.UseMiddleware(CreateController, MiddlewareStages.PostBindValuesPreInvoke);
                    c.UseMiddleware(BuildBullseyeTargets, MiddlewareStages.PostBindValuesPreInvoke);
                    c.BuildEvents.OnCommandCreated += AddControllerOptions;
                })
                //.UseErrorHandler((_, _) => ExitCodes.Error.Result)
               // .UseDefaultsFromEnvVar()
                .RunAsync(args).ConfigureAwait(true);
        }
    }
}

