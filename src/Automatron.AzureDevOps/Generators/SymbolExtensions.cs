using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

public static class SymbolExtensions
{
    private static readonly Dictionary<string, Attribute> Cache = new();

    private static T MapToType<T>(AttributeData attributeData) where T : Attribute
    {
        return MapToType<T>(attributeData, typeof(T));
    }

    private static T MapToType<T>(AttributeData attributeData, Type type) where T : Attribute
    {
        /*var key = attributeData.AttributeClass!.Name;
        if (Cache.TryGetValue(key, out var value))
        {   
            return (T)value;
        }*/

        T attribute;
        if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
        {
            attribute = (T)Activator.CreateInstance(type, GetActualConstructorParams(attributeData).ToArray())!;
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
       // Cache.Add(key, attribute);
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

        return typeString switch
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
            "System.Type" => typedConstant.Value!.ToString(),
            _ => throw new NotSupportedException()
        };
    }

    private static IEnumerable<object?> GetActualConstructorParams(AttributeData attributeData)
    {
        return attributeData.ConstructorArguments.Select(GetTypedValue);
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.AttributeClass!.Name == typeof(T).Name).Select(MapToType<T>);
    }

    public static IEnumerable<T> GetCustomAbstractAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.AttributeClass!.BaseType!.Name == typeof(T).Name).Select(c=> MapToType<T>(c, Type.GetType(c.AttributeClass + ", " + c.AttributeClass?.ContainingAssembly) ?? throw new InvalidOperationException()));
    }

}