#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Reflection;
using Automatron.Tasks.Models;

namespace Automatron.Tasks.Middleware;

internal class ParameterTypeVisitor : MemberVisitor<IEnumerable<ParameterType>>
{
    public override IEnumerable<ParameterType> VisitType(Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            foreach (var constructorParameter in constructor.GetParameters())
            {
                var parameters = constructorParameter.ParameterType.Accept(this) ?? Enumerable.Empty<ParameterType>();

                foreach (var parameter in parameters)
                {
                    yield return parameter;
                }
            }
        }

        yield return new ParameterType(type, (type.Accept(new ParameterVisitor()) ?? Enumerable.Empty<Parameter>()).ToArray());
    }
}
#endif