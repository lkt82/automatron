#if NET6_0
using System;
using System.Collections.Generic;

namespace Automatron.Models;

public interface ITypeProvider
{
    public IEnumerable<Type> Types { get;}
}
#endif