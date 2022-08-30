using System;

namespace Automatron;

#if NET6_0
internal abstract record Action(Type Type)
{
    public abstract object? Invoke(object service);
}
#endif