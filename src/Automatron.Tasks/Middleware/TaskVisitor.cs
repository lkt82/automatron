#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.Models;
using Automatron.Reflection;
using Automatron.Tasks.Annotations;
using Automatron.Tasks.Models;

namespace Automatron.Tasks.Middleware;

internal class TaskVisitor : MemberVisitor<IEnumerable<Task>>
{
    private readonly List<Type> _visitedTypes = new();

    private readonly List<MethodInfo> _visitedMethods = new();

    private readonly Dictionary<MemberInfo, Task> _createdTasks = new();

    private readonly Dictionary<MemberInfo, HashSet<MemberInfo>> _dependsOn = new();

    public IEnumerable<Task> VisitTypes(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var tasks = type.Accept(this);

            if (tasks == null)
            {
                continue;
            }

            var parameterTypes = (type.Accept(new ParameterTypeVisitor()) ?? Enumerable.Empty<ParameterType>()).ToArray();

            foreach (var task in tasks)
            {
                task.ParameterTypes = parameterTypes;

                yield return task;
            }
        }

        foreach (var task in _createdTasks)
        {
            if (!_dependsOn.ContainsKey(task.Key))
            {
                continue;
            }

            foreach (var memberInfo in _dependsOn[task.Key])
            {
                task.Value.Dependencies.Add(_createdTasks[memberInfo]);
            }
        }
    }

    public override IEnumerable<Task> VisitType(Type type)
    {
        if (_visitedTypes.Contains(type))
        {
            yield break;
        }

        _visitedTypes.Add(type);

        var taskAttribute = type.GetAllCustomAttribute<TaskAttribute>();

        if (taskAttribute != null)
        {
            var dependencies = VisitDependencies(type);

            foreach (var task in dependencies)
            {
                yield return task;
            }

            yield return CreateTask(type, taskAttribute, new HashSet<Task>());
        }

        var methods = type.GetAllMethods();

        foreach (var method in methods)
        {
            var tasks = method.Accept(this);

            if (tasks == null)
            {
                continue;
            }

            foreach (var task in tasks)
            {
                yield return task;
            }
        }
    }

    public override IEnumerable<Task> VisitMethod(MethodInfo methodInfo)
    {
        if (_visitedMethods.Contains(methodInfo))
        {
            yield break;
        }

        _visitedMethods.Add(methodInfo);

        foreach (var taskAttribute in methodInfo.GetCustomAttributes<TaskAttribute>())
        {
            var dependencies = VisitDependencies(methodInfo);

            foreach (var task in dependencies)
            {
                yield return task;
            }

            yield return CreateTask(methodInfo, methodInfo, taskAttribute, new HashSet<Task>());
        }
    }

    private IEnumerable<Task> VisitDependencies(MemberInfo memberInfo)
    {
        var type = memberInfo.ReflectedType!;

        foreach (var dependentOnAttribute in memberInfo.GetAllCustomAttributes<DependentOnAttribute>())
        {
            if (dependentOnAttribute.Type == null)
            {
                foreach (var taskName in dependentOnAttribute.Actions)
                {
                    var method = type.GetMethod(taskName) ?? throw new InvalidOperationException();

                    var tasks = method.Accept(this);

                    if (!_dependsOn.ContainsKey(memberInfo))
                    {
                        _dependsOn[memberInfo] = new HashSet<MemberInfo>();
                    }
                    _dependsOn[memberInfo].Add(method);

                    if (tasks == null)
                    {
                        continue;
                    }

                    foreach (var task in tasks)
                    {
                        yield return task;
                    }
                }
            }
            else
            {
                var tasks = dependentOnAttribute.Type.Accept(this);

                if (!_dependsOn.ContainsKey(memberInfo))
                {
                    _dependsOn[memberInfo] = new HashSet<MemberInfo>();
                }
                _dependsOn[memberInfo].Add(dependentOnAttribute.Type);

                if (tasks == null)
                {
                    yield break;
                }

                foreach (var task in tasks)
                {
                    yield return task;
                }
            }
        }

        foreach (var dependentForAttribute in memberInfo.GetAllCustomAttributes<DependentForAttribute>())
        {
            if (dependentForAttribute.Type == null)
            {
                foreach (var taskName in dependentForAttribute.Actions)
                {
                    var method = type.GetMethod(taskName) ?? throw new InvalidOperationException();

                    var tasks = method.Accept(this);

                    if (!_dependsOn.ContainsKey(method))
                    {
                        _dependsOn[method] = new HashSet<MemberInfo>();
                    }
                    _dependsOn[method].Add(memberInfo);

                    if (tasks == null)
                    {
                        continue;
                    }

                    foreach (var task in tasks)
                    {
                        yield return task;
                    }
                }
            }
            else
            {
                var tasks = dependentForAttribute.Type.Accept(this);

                if (!_dependsOn.ContainsKey(dependentForAttribute.Type))
                {
                    _dependsOn[dependentForAttribute.Type] = new HashSet<MemberInfo>();
                }
                _dependsOn[dependentForAttribute.Type].Add(memberInfo);

                if (tasks == null)
                {
                    yield break;
                }

                foreach (var task in tasks)
                {
                    yield return task;
                }
            }
        }
    }

    private Task CreateTask(Type type,TaskAttribute taskAttribute, ISet<Task> dependencies)
    {
        if (string.IsNullOrEmpty(taskAttribute.Action))
        {
            throw new InvalidOperationException();
        }

        return CreateTask(type,type.GetMethod(taskAttribute.Action) ?? throw new InvalidOperationException(), taskAttribute, dependencies);

    }

    private Task CreateTask(MemberInfo member, MethodInfo methodInfo, TaskAttribute taskAttribute, ISet<Task> dependencies)
    {
        var name = !string.IsNullOrEmpty(taskAttribute.Name) ? taskAttribute.Name : member.Name;

        var type = methodInfo.ReflectedType!;

        var task = new Task(name, dependencies, new MethodAction(methodInfo), new List<ParameterType>(), type)
        {
            Default = taskAttribute.Default
        };

        _createdTasks[member] = task;

        return task;
    }
}

#endif