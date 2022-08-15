using System;

namespace Automatron;

#if NET6_0
internal abstract record ActionDescriptor(Type Type)
{
    public abstract object? Invoke(object service);
}
#endif