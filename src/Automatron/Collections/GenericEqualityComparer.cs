using System;
using System.Collections.Generic;

namespace Automatron.Collections;

internal class GenericEqualityComparer<TItem, TKey>(Func<TItem, TKey> getKey) : EqualityComparer<TItem>
    where TKey : notnull
{
    private readonly EqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;

    public override bool Equals(TItem? x, TItem? y)
    {
        if (x == null && y == null)
        {
            return true;
        }
        if (x == null || y == null)
        {
            return false;
        }
        return _keyComparer.Equals(getKey(x), getKey(y));
    }

    public override int GetHashCode(TItem obj)
    {
        if (obj == null)
        {
            return 0;
        }
        return _keyComparer.GetHashCode(getKey(obj));
    }
}
