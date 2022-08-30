using System.Reflection;

namespace Automatron;

#if NET6_0
internal record ParameterProperty(Parameter Parameter, PropertyInfo Property)
{
}
#endif