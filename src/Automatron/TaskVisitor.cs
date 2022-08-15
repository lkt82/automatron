#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Automatron.Annotations;
using Automatron.Reflection;

namespace Automatron;

internal class TaskVisitor: SymbolVisitor
{
    private readonly IEnumerable<Type> _allowTypes;

    public TaskVisitor(IEnumerable<Type> allowTypes)
    {
        _allowTypes = allowTypes;
    }

    public Dictionary<string,Task> Tasks { get; } = new();

    public Dictionary<string, Parameter> Parameters { get; } = new();

    private Dictionary<string, HashSet<ParameterDescriptor>> TypeParameters { get; } = new();

    private Dictionary<string, HashSet<ParameterTypeDescriptor>> ParameterDependencies { get; } = new();

    private readonly HashSet<object> _visited = new();

    public Type? Type { get; set; }

    private PropertyInfo? Property { get; set; }

    public Task? Task { get; set; }

    public Type? OwnerType { get; set; }

    public Task? ParentTask { get; set; }

    public override void VisitType(Type type)
    {
        if (!_allowTypes.Contains(type))
        {
            return;
        }

        Type = type;

        if (!type.IsNested && !type.IsAbstract)
        {
            OwnerType = type;
            ParentTask = null;
        }

        var id = GetId(type);

        if (_visited.Contains(id))
        {
            return;
        }

        _visited.Add(id);

        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                var currentOwner = OwnerType;

                OwnerType = null;

                parameter.ParameterType.Accept(this);

                var typeId = GetId(parameter.ParameterType);

                var parameterTypeDescriptors = new HashSet<ParameterTypeDescriptor>();

                var parameters = TypeParameters.TryGetValue(typeId, out var value) ? value : Enumerable.Empty<ParameterDescriptor>();

                parameterTypeDescriptors.Add(new ParameterTypeDescriptor(parameter.ParameterType, parameters));

                ParameterDependencies.Add(id, parameterTypeDescriptors);

                OwnerType = currentOwner;
            }
        }

        var taskAttribute = type.GetCachedAttribute<TaskAttribute>();

        if (taskAttribute != null)
        {
            Task task;

            var parameters = new HashSet<ParameterTypeDescriptor>(ParameterDependencies.TryGetValue(GetId(type), out var value2) ? value2 : Enumerable.Empty<ParameterTypeDescriptor>())
            {
                new(type,
                    TypeParameters.TryGetValue(GetId(type), out var value) ? value : Enumerable.Empty<ParameterDescriptor>())
            };

            if (taskAttribute.Action == null)
            {
                task = new Task(GetName(type, taskAttribute), new HashSet<Task>(), new EmptyActionDescriptor(type), parameters)
                {
                    Default = taskAttribute.Default
                };
            }
            else
            {
                var methodInfo = type.GetMethod(taskAttribute.Action) ?? throw new InvalidOperationException();

                task = new Task(GetName(type, taskAttribute), new HashSet<Task>(), new MethodActionDescriptor(methodInfo, type), parameters)
                {
                    Default = taskAttribute.Default
                };
            }

            Tasks.Add(id, task);

            Task = task;

            Task = task;
            ParentTask = task;
        }

        foreach (var attribute in type.GetCustomAttributes())
        {
            attribute.Accept(this);
        }

        foreach (var property in type.GetProperties().Where(c => c.DeclaringType != typeof(object) && !c.IsSpecialName))
        {
            property.Accept(this);
        }

        foreach (var method in type.GetMethods().Where(c=> c.DeclaringType != typeof(object) && !c.IsSpecialName))
        {
            method.Accept(this);
        }

        foreach (var method in type.GetInterfaces().SelectMany(c => c.GetMethods()))
        {
            method.Accept(this);
        }

        foreach (var nestedType in type.GetNestedTypes())
        {
            nestedType.Accept(this);
        }

        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            foreach (var nestedType in type.BaseType.GetNestedTypes())
            {
                nestedType.Accept(this);
            }
        }
    }

    public override void VisitMethod(MethodInfo methodInfo)
    {
        var id = GetId(methodInfo);

        if (_visited.Contains(id))
        {
            return;
        }

        _visited.Add(id);


        var taskAttribute = methodInfo.GetCachedAttribute<TaskAttribute>();
        if (taskAttribute == null)
        {
            return;
        }

        var type = methodInfo.ReflectedType!.IsInterface || methodInfo.ReflectedType!.IsAbstract ? Type! : methodInfo.ReflectedType;

        var parameters = new HashSet<ParameterTypeDescriptor>(ParameterDependencies.TryGetValue(GetId(type), out var value2) ? value2 : Enumerable.Empty<ParameterTypeDescriptor>())
        {
            new(type,
                TypeParameters.TryGetValue(GetId(type), out var value) ? value : Enumerable.Empty<ParameterDescriptor>())
        };

        var task = new Task(GetName(methodInfo, taskAttribute), new HashSet<Task>(), new MethodActionDescriptor(methodInfo, type), parameters)
        {
            Default = taskAttribute.Default
        };

        Tasks.Add(id, task);

        Task = task;

        foreach (var attribute in methodInfo.GetCustomAttributes())
        {
            attribute.Accept(this);
        }
    }

    public string GetName(MemberInfo member, TaskAttribute taskAttribute)
    {
        var tokens = new List<string>();
        if (ParentTask != null)
        {
            tokens.Add(ParentTask.Name);
        }

        tokens.Add(string.IsNullOrEmpty(taskAttribute.Name) ? member.Name : taskAttribute.Name);

        return string.Join('-', tokens);
    }



    public string GetId(MemberInfo member)
    {
        var tokens = new HashSet<string>();
        if (member is Type type)
        {
            if (OwnerType != type)
            {
                tokens.Add(OwnerType!.FullName!);
                tokens.Add(type.FullName!);
            }
            else
            {
                tokens.Add(type.FullName!);
            }
        }

        if (member is not System.Type)
        {
            if (OwnerType != member.ReflectedType)
            {
                tokens.Add(OwnerType!.FullName!);
                tokens.Add(member.ReflectedType!.FullName!);
                tokens.Add(member.Name);
            }
            else
            {
                tokens.Add(member.ReflectedType!.FullName!);
            }
            tokens.Add(member.Name);
        }

        return string.Join('+',tokens);
     }

    public override void VisitAttribute(Attribute attribute)
    {
        switch (attribute)
        {
            case DependentForAttribute dependentForAttribute:
                VisitDependentForAttribute(dependentForAttribute);
                break;
            case DependentOnAttribute dependentOnAttribute:
                VisitDependentOnAttribute(dependentOnAttribute);
                break;
            case ParameterAttribute parameterAttribute:
                VisitParameterAttribute(parameterAttribute);
                break;
        }
    }

    private void VisitParameterAttribute(ParameterAttribute parameterAttribute)
    {
        var property = Property!;

        var tokens = new HashSet<string>();

        if (ParentTask != null)
        {
            tokens.Add(ParentTask.Name);
        }

        tokens.Add(!string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : property.Name);

        var name = string.Join('-', tokens);

        var type = property.ReflectedType!.IsInterface || property.ReflectedType!.IsAbstract ? Type! : property.ReflectedType;

        var typeId = GetId(type);

        var parameter = new Parameter(name, parameterAttribute.Description, property.PropertyType);

        var descriptor = new ParameterDescriptor(parameter, property);

        if (!TypeParameters.ContainsKey(typeId))
        {
            TypeParameters.Add(typeId, new HashSet<ParameterDescriptor>());
        }

        TypeParameters[typeId].Add(descriptor);

        Parameters.Add(GetId(Property!), parameter);
    }

    public override void VisitProperty(PropertyInfo propertyInfo)
    {
        var id = GetId(propertyInfo);

        if (_visited.Contains(id))
        {
            return;
        }

        _visited.Add(id);

        foreach (var attribute in propertyInfo.GetCustomAttributes())
        {
            Property = propertyInfo;

            attribute.Accept(this);
        }

        Property = propertyInfo;
        //propertyInfo.PropertyType.Accept(this);
    }

    public virtual void VisitDependentForAttribute(DependentForAttribute dependentForAttribute)
    {
        var currentType = Type!;
        var currentTask = Task!;

        if (dependentForAttribute.Type == null)
        {
            foreach (var task in dependentForAttribute.Actions)
            {
                (currentType.GetMethod(task) ?? throw new InvalidOperationException()).Accept(this);
            }
        }
        else
        {
            dependentForAttribute.Type.Accept(this);
        }

        var dependentType = dependentForAttribute.Type ?? currentType;

        if (dependentForAttribute.Actions.Length == 0 && dependentForAttribute.Type != null)
        {
            var dependentKey = GetId(dependentType);

            var task = Tasks[dependentKey];

            task.Dependencies.Add(currentTask);
        }

        foreach (var methodName in dependentForAttribute.Actions)
        {
            var dependentKey = GetId(dependentType.GetMethod(methodName) ?? throw new InvalidOperationException());

            var task = Tasks[dependentKey];

            task.Dependencies.Add(currentTask);
        }
    }

    public virtual void VisitDependentOnAttribute(DependentOnAttribute dependentOnAttribute)
    {
        var currentType = Type!;
        var currentTask = Task!;

        if (dependentOnAttribute.Type == null)
        {
            foreach (var task in dependentOnAttribute.Actions)
            {
                (currentType.GetMethod(task) ?? throw new InvalidOperationException()).Accept(this);
            }
        }
        else
        {
            dependentOnAttribute.Type.Accept(this);
        }

        var dependentType = dependentOnAttribute.Type ?? currentType;

        if (dependentOnAttribute.Actions.Length == 0 && dependentOnAttribute.Type != null)
        {
            var dependentKey = GetId(dependentType);

            var task = Tasks[dependentKey];

            currentTask.Dependencies.Add(task);
        }

        foreach (var methodName in dependentOnAttribute.Actions)
        {
            var dependentKey = GetId(dependentType.GetMethod(methodName) ?? throw new InvalidOperationException());

            var task = Tasks[dependentKey];

            currentTask.Dependencies.Add(task);
        }
    }
}
#endif