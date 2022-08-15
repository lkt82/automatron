using System.Reflection;

namespace Automatron;

#if NET6_0
internal record ParameterDescriptor(Parameter Parameter, PropertyInfo Property)
{
}
#endif