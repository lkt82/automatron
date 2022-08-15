using System;

namespace Automatron;

#if NET6_0
internal record EmptyActionDescriptor(Type Type) : ActionDescriptor(Type)
{
    public override object? Invoke(object service)
    {
        return null;
    }
}
#endif