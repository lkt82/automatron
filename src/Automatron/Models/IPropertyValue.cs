#if NET6_0
using System.Reflection;

namespace Automatron.Models;

public interface IPropertyValue
{
    public PropertyInfo Property { get; }

    public object? Value { get; set; }
}
#endif