#if NET6_0
using System;
using System.Reflection;

namespace Automatron;

internal class TaskAttributeProvider : ICustomAttributeProvider
{
    private readonly PropertyInfo _propertyInfo;

    public TaskAttributeProvider(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }

    public object[] GetCustomAttributes(bool inherit)
    {
        return _propertyInfo.GetCustomAttributes(inherit);
    }

    public object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _propertyInfo.GetCustomAttributes(attributeType,inherit);
    }

    public bool IsDefined(Type attributeType, bool inherit)
    {
        return _propertyInfo.IsDefined(attributeType, inherit);
    }
}

#endif