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

    public TaskVisitor(ITypeProvider allowTypes)
    {
        _allowTypes = allowTypes.Types;
    }

    public Dictionary<string,Task> Tasks { get; } = new();

    public Dictionary<string, Parameter> Parameters { get; } = new();

    protected Dictionary<string, HashSet<ParameterDescriptor>> TypeParameters { get; } = new();

    protected Dictionary<string, HashSet<ParameterTypeDescriptor>> ParameterDependencies { get; } = new();

    private readonly HashSet<object> _visited = new();

    protected Type? Type { get; set; }

    protected PropertyInfo? Property { get; set; }

    protected MethodInfo? MethodInfo { get; set; }

    protected Task? Task { get; set; }

    protected Type? RootType { get; set; }

    protected Task? ParentTask { get; set; }

    private string? Path { get; set; }

    public override void VisitType(Type type)
    {
        if (!_allowTypes.Contains(type))
        {
            return;
        }

        Type = type;
        MethodInfo = null;
        Property = null;

        if (!type.IsNested && !type.IsAbstract)
        {
            RootType = type;
            ParentTask = null;
        }

        var path = GetPath(type);

        Path = path;

        if (_visited.Contains(path))
        {
            return;
        }

        _visited.Add(path);

        foreach (var constructor in type.GetConstructors())
        {
            constructor.Accept(this);

            Type = type;
        }

        foreach (var attribute in GetAttributes(type))
        {
            attribute.Accept(this);

            Type = type;
        }

        foreach (var property in type.GetProperties().Where(c => c.DeclaringType != typeof(object) && !c.IsSpecialName))
        {
            property.Accept(this);

            Type = type;
        }

        foreach (var method in type.GetMethods().Where(c=> c.DeclaringType != typeof(object) && !c.IsSpecialName))
        {
            method.Accept(this);

            Type = type;
        }

        foreach (var method in type.GetInterfaces().SelectMany(c => c.GetMethods()))
        {
            method.Accept(this);

            Type = type;
        }

        var nestedTypes = type.GetNestedTypes();


        foreach (var nestedType in nestedTypes)
        {
            var parentTask = ParentTask;
   
            nestedType.Accept(this);

            ParentTask = parentTask;
            Path = path;
            Type = type;
        }

        if (type.BaseType != null && type.BaseType != typeof(object))
        {
            var baseNestedTypes = type.BaseType.GetNestedTypes();

            foreach (var nestedType in baseNestedTypes)
            {
                var parentTask = ParentTask;
       
                nestedType.Accept(this);

                ParentTask = parentTask;
                Path = path;
                Type = type;
            }
        }
    }

    protected virtual IEnumerable<Attribute> GetAttributes(MemberInfo member)
    {
        var attributes = new List<Attribute>();

        Attribute? attribute = member.GetCachedAttribute<TaskAttribute>();

        if (attribute != null)
        {
            attributes.Add(attribute);
        }

        attribute = member.GetCachedAttribute<DependentOnAttribute>();

        if (attribute != null)
        {
            attributes.Add(attribute);
        }

        attribute = member.GetCachedAttribute<DependentForAttribute>();

        if (attribute != null)
        {
            attributes.Add(attribute);
        }

        return attributes;
    }

    public override void VisitMethod(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;

        var id = GetPath(methodInfo);

        if (_visited.Contains(id))
        {
            return;
        }

        _visited.Add(id);

        foreach (var attribute in GetAttributes(methodInfo))
        {
            attribute.Accept(this);

            MethodInfo = methodInfo;
        }
    }

    protected string GetName(MemberInfo member, string? name)
    {
        var tokens = new List<string>();
        if (ParentTask != null)
        {
            tokens.Add(ParentTask.Name);
        }

        tokens.Add(string.IsNullOrEmpty(name) ? member.Name : name);

        return string.Join('-', tokens);
    }

    public string GetPath(MemberInfo member)
    {
        var tokens = new List<string>();
        if (member is Type type)
        {
            var currentType = type;

            while (currentType != null)
            {
                if (currentType.IsAbstract)
                {
                    break;
                }

                var typeToken = "/" + currentType.Namespace + "/" + currentType.Name;

                tokens.Add(typeToken);

                if (currentType.ReflectedType is { IsAbstract: true } && Path != null && Path.EndsWith(typeToken))
                {
                    return Path;
                }

                if (RootType != null && currentType.IsNested && currentType.ReflectedType is { IsAbstract: true })
                {
                    tokens.Add("/" + RootType.Namespace + "/" + RootType.Name);
                }

                currentType = currentType.ReflectedType;
            }
        }

        if (member is not System.Type)
        {
            tokens.Add("/" + member.Name);

            var currentType = member.ReflectedType;

            while (currentType != null)
            {
                if (currentType.IsAbstract)
                {
                    break;
                }

                tokens.Add("/" + currentType.Namespace + "/" + currentType.Name);

                if (RootType != null && currentType.IsNested && currentType.ReflectedType is { IsAbstract: true })
                {
                    tokens.Add("/" + RootType.Namespace + "/" + RootType.Name);
                }

                currentType = currentType.ReflectedType;
            }
        }

        tokens.Reverse();
        return string.Concat(tokens);
    }

    public override void VisitConstructor(ConstructorInfo constructorInfo)
    {
        var type = Type!;

        var id = GetPath(type);

        foreach (var parameter in constructorInfo.GetParameters())
        {
            var currentOwner = RootType;

            RootType = null;

            var path = Path;

            parameter.ParameterType.Accept(this);

            var typeId = GetPath(parameter.ParameterType);

            var parameterTypeDescriptors = new HashSet<ParameterTypeDescriptor>();

            var parameters = TypeParameters.TryGetValue(typeId, out var value) ? value : Enumerable.Empty<ParameterDescriptor>();

            parameterTypeDescriptors.Add(new ParameterTypeDescriptor(parameter.ParameterType, parameters));

            ParameterDependencies.Add(id, parameterTypeDescriptors);

            RootType = currentOwner;
            Type = type;
            Path = path;
        }
    }

    public override void VisitAttribute(Attribute attribute)
    {
        switch (attribute)
        {
            case TaskAttribute taskAttribute:
                VisitTaskAttribute(taskAttribute);
                break;
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
        var property = Property;

        if (property == null)
        {
            return;
        }

        // Enable nested parameters
        //var tokens = new HashSet<string>();

        //if (ParentTask != null)
        //{
        //    tokens.Add(ParentTask.Name);
        //}

        //tokens.Add(!string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : property.Name);

        //var name = string.Join('-', tokens);
        var name = !string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : property.Name;

        AddParameter(name, parameterAttribute.Description, property);
    }

    protected void AddParameter(string name,string? description, PropertyInfo property)
    {
        var type = property.ReflectedType!.IsInterface || property.ReflectedType!.IsAbstract ? Type! : property.ReflectedType;

        var typeId = GetPath(type);

        var parameter = new Parameter(name, description, property.PropertyType);

        var descriptor = new ParameterDescriptor(parameter, property);

        if (!TypeParameters.ContainsKey(typeId))
        {
            TypeParameters.Add(typeId, new HashSet<ParameterDescriptor>());
        }

        TypeParameters[typeId].Add(descriptor);

        var propertyId = GetPath(property);

        Parameters.Add(propertyId, parameter);
    }

    public override void VisitProperty(PropertyInfo propertyInfo)
    {
        var id = GetPath(propertyInfo);

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

    public virtual void VisitTaskAttribute(TaskAttribute taskAttribute)
    {
        if (MethodInfo != null)
        {
            var type = MethodInfo.ReflectedType!.IsInterface || MethodInfo.ReflectedType!.IsAbstract ? Type! : MethodInfo.ReflectedType;

            var methodId = GetPath(MethodInfo);

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            var task = new Task(GetName(MethodInfo, taskAttribute.Name), new HashSet<Task>(), new MethodActionDescriptor(MethodInfo, type), parameters)
            {
                Default = taskAttribute.Default
            };

            Tasks.Add(methodId, task);

            Task = task;
        }
        else
        {
            var type = Type!;

            var typeId = GetPath(type);

            var parameters = GetParameterTypeDescriptors(typeId, type);

            Task task;

            if (taskAttribute.Action == null)
            {
                task = new Task(GetName(type, taskAttribute.Name), new HashSet<Task>(), new EmptyActionDescriptor(type),
                    parameters)
                {
                    Default = taskAttribute.Default
                };
            }
            else
            {
                var methodInfo = type.GetMethod(taskAttribute.Action) ?? throw new InvalidOperationException();

                task = new Task(GetName(type, taskAttribute.Name), new HashSet<Task>(),
                    new MethodActionDescriptor(methodInfo, type), parameters)
                {
                    Default = taskAttribute.Default
                };
            }

            Tasks.Add(typeId, task);

            Task = task;

            ParentTask = task;
        }
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
            var path = Path;

            Path = "";

            dependentForAttribute.Type.Accept(this);

            Path = path;
        }

        var dependentType = dependentForAttribute.Type ?? currentType;

        if (dependentForAttribute.Actions.Length == 0 && dependentForAttribute.Type != null)
        {
            var dependentKey = GetPath(dependentType);

            var task = Tasks[dependentKey];

            task.Dependencies.Add(currentTask);
        }

        foreach (var methodName in dependentForAttribute.Actions)
        {
            var dependentKey = GetPath(dependentType.GetMethod(methodName) ?? throw new InvalidOperationException());

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
            var path = Path;

            dependentOnAttribute.Type.Accept(this);

            Path = path;
        }

        var dependentType = dependentOnAttribute.Type ?? currentType;

        if (dependentOnAttribute.Actions.Length == 0 && dependentOnAttribute.Type != null)
        {
            var dependentKey = GetPath(dependentType);

            var task = Tasks[dependentKey];

            currentTask.Dependencies.Add(task);
        }

        foreach (var methodName in dependentOnAttribute.Actions)
        {
            var dependentKey = GetPath(dependentType.GetMethod(methodName) ?? throw new InvalidOperationException());

            var task = Tasks[dependentKey];

            currentTask.Dependencies.Add(task);
        }
    }

    protected HashSet<ParameterTypeDescriptor> GetParameterTypeDescriptors(string id, Type type)
    {
        if (!TypeParameters.ContainsKey(id))
        {
            TypeParameters.Add(id, new HashSet<ParameterDescriptor>());
        }

        var parameters = new HashSet<ParameterTypeDescriptor>(
            ParameterDependencies.TryGetValue(id, out var value2)
                ? value2
                : Enumerable.Empty<ParameterTypeDescriptor>())
        {
            new(type,
                TypeParameters.TryGetValue(id, out var value)
                    ? value
                    : Enumerable.Empty<ParameterDescriptor>())
        };
        return parameters;
    }
}
#endif