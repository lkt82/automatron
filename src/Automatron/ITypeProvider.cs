﻿#if NET6_0
using System;
using System.Collections.Generic;

namespace Automatron;

internal interface ITypeProvider
{
    public IEnumerable<Type> GetTypes();
}
#endif