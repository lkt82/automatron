using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

public static class AttributeDataExtensions
{
    public static T MapToCustomAttribute<T>(this AttributeData attributeData) where T : Attribute
    {
        return MapToCustomAttribute<T>(attributeData, typeof(T));
    }

    public static T MapToCustomAttribute<T>(this AttributeData attributeData, Type type) where T : Attribute
    {
        T attribute;
        if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
        {
            var parameters = GetActualConstructorParams(attributeData).ToArray();

            attribute = (T)Activator.CreateInstance(type, BindingFlags.Public|BindingFlags.NonPublic | BindingFlags.Instance,null, parameters,null)!;
        }
        else
        {
            attribute = (T)Activator.CreateInstance(type)!;
        }
        // ReSharper disable once UseDeconstruction
        foreach (var keyValue in attributeData.NamedArguments)
        {
            var property = type.GetProperty(keyValue.Key);

            if (property == null)
            {
                throw new Exception();
            }

            property.SetValue(attribute, GetTypedValue(keyValue.Value));
        }
        return attribute;
    }

    private static object GetTypedValue(TypedConstant typedConstant)
    {
        if (typedConstant.Type != null &&
            typedConstant.Kind != TypedConstantKind.Primitive &&
            typedConstant.Kind != TypedConstantKind.Array &&
            typedConstant.Kind != TypedConstantKind.Enum &&
            typedConstant.Kind != TypedConstantKind.Type
           )
            throw new NotSupportedException();

        var typeString = typedConstant.Type!.ToString();

        if (typedConstant.Kind == TypedConstantKind.Enum)
        {
            typeString = typedConstant.Type!.BaseType!.ToString();
        }

        return (typeString switch
        {
            "string[]" => typedConstant.Values.Select(a => a.Value).OfType<string>().ToArray(),
            "int[]" => typedConstant.Values.Select(a => a.Value).OfType<int>().ToArray(),
            "DateTime[]" => typedConstant.Values.Select(a => a.Value).OfType<DateTime>().ToArray(),
            "bool[]" => typedConstant.Values.Select(a => a.Value).OfType<bool>().ToArray(),
            "string" => typedConstant.Value!,
            "bool" => typedConstant.Value!,
            "int" => typedConstant.Value!,
            "DateTime" => typedConstant.Value!,
            "System.Enum" => Enum.ToObject(Type.GetType(typedConstant.Type!.ToString()!)!, typedConstant.Value!),
            "System.Type" => (INamedTypeSymbol)typedConstant.Value!,
            _ => throw new NotSupportedException()
        })!;
    }
    private static IEnumerable<object?> GetActualConstructorParams(AttributeData attributeData)
    {
        return attributeData.ConstructorArguments.Select(GetTypedValue);
    }

    public static bool IsCustomAttribute<T>(this AttributeData attributeData) where T : Attribute
    {
        return attributeData.AttributeClass!.Name == typeof(T).Name;
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this IEnumerable<AttributeData> attributeData) where T : Attribute
    {
        return attributeData.Where(c => c.IsCustomAttribute<T>()).Select(c => c.MapToCustomAttribute<T>());
    }

    public static bool IsAssignableFrom<T>(this AttributeData attribute)
    {
        var current = attribute.AttributeClass;
        while (current != null && current.Name != "Attribute")
        {
            if (current.Name == typeof(T).Name)
            {
                return true;
            }
            current = current.BaseType;
        }

        return false;
    }

}