using System;

namespace Automatron;

#if NET6_0
internal record EmptyAction(Type Type) : Action(Type)
{
    public override object? Invoke(object service)
    {
        return null;
    }
}
#endif