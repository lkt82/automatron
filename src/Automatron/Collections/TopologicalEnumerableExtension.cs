using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Automatron.Collections;

public static class TopologicalEnumerableExtension
{
    private static Func<T, IEnumerable<T>> RemapDependencies<T, TKey>(IEnumerable<T> source, Func<T, IEnumerable<TKey>> getDependencies, Func<T, TKey> getKey) where TKey : notnull
    {
        var map = source.ToDictionary(getKey);
        return item =>
        {
            var dependencies = getDependencies(item);
            return dependencies.Select(key => map[key]);
        };
    }

    public static IList<T> TopologicalSort<T, TKey>(this IEnumerable<T> source, Func<T, IEnumerable<TKey>> getDependencies, Func<T, TKey> getKey, bool ignoreCycles = false) where T : notnull where TKey : notnull
    {
        var source2 = source as ICollection<T> ?? source.ToArray();
        return TopologicalSort<T>(source2, RemapDependencies(source2, getDependencies, getKey), null, ignoreCycles);
    }

    public static IList<T> TopologicalSort<T, TKey>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, Func<T, TKey> getKey, bool ignoreCycles = false) where T : notnull where TKey : notnull
    {
        return TopologicalSort(source, getDependencies, new GenericEqualityComparer<T, TKey>(getKey), ignoreCycles);
    }

    public static IList<T> TopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, IEqualityComparer<T>? comparer = null, bool ignoreCycles = false) where T : notnull
    {
        var sorted = new List<T>();
        var visited = new Dictionary<T, bool>(comparer);

        foreach (var item in source)
        {
            Visit(item, getDependencies, sorted, visited, ignoreCycles);
        }

        return sorted;
    }

    private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, ICollection<T> sorted, IDictionary<T, bool> visited, bool ignoreCycles) where T : notnull
    {
        var alreadyVisited = visited.TryGetValue(item, out var inProcess);

        if (alreadyVisited)
        {
            if (inProcess && !ignoreCycles)
            {
                throw new ArgumentException("Cyclic dependency found.");
            }
        }
        else
        {
            visited[item] = true;

            var dependencies = getDependencies(item);
            foreach (var dependency in dependencies)
            {
                Visit(dependency, getDependencies, sorted, visited, ignoreCycles);
            }

            visited[item] = false;
            sorted.Add(item);
        }
    }

    public static IList<ICollection<T>> Group<T, TKey>(IEnumerable<T> source, Func<T, IEnumerable<TKey>> getDependencies, Func<T, TKey> getKey, bool ignoreCycles = true) where T : notnull where TKey : notnull
    {
        var source2 = source as ICollection<T> ?? source.ToList();
        return Group<T>(source2, RemapDependencies(source2, getDependencies, getKey), null, ignoreCycles);
    }

    public static IList<ICollection<T>> Group<T, TKey>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, Func<T, TKey> getKey, bool ignoreCycles = true) where T : notnull where TKey : notnull
    {
        return Group(source, getDependencies, new GenericEqualityComparer<T, TKey>(getKey), ignoreCycles);
    }

    public static IList<ICollection<T>> Group<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, IEqualityComparer<T>? comparer = null, bool ignoreCycles = true) where T : notnull
    {
        var sorted = new List<ICollection<T>>();
        var visited = new Dictionary<T, int>(comparer);

        foreach (var item in source)
        {
            Visit(item, getDependencies, sorted, visited, ignoreCycles);
        }

        return sorted;
    }

    public static int Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<ICollection<T>> sorted, Dictionary<T, int> visited, bool ignoreCycles) where T : notnull
    {
        const int inProcess = -1;
        var alreadyVisited = visited.TryGetValue(item, out var level);

        if (alreadyVisited)
        {
            if (level == inProcess && ignoreCycles)
            {
                throw new ArgumentException("Cyclic dependency found.");
            }
        }
        else
        {
            visited[item] = level = inProcess;

            var dependencies = getDependencies(item);
            foreach (var dependency in dependencies)
            {
                var depLevel = Visit(dependency, getDependencies, sorted, visited, ignoreCycles);
                level = Math.Max(level, depLevel);
            }

            visited[item] = ++level;
            while (sorted.Count <= level)
            {
                sorted.Add(new Collection<T>());
            }
            sorted[level].Add(item);
        }

        return level;
    }

}
