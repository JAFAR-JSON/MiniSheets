using MiniSheets.Core.Model;
using MiniSheets.Shared;

namespace MiniSheets.Core.Formula;

/// <summary>
/// Topological sorting using Kahn's algorhitm.
/// Works on value types only.
/// Have to manually sort after every change.
/// https://rosettacode.org/wiki/Topological_sort#C#
/// </summary>
public class DependencyGraph<ValueType> where ValueType : notnull
{
    private class Relations
    {
        public HashSet<ValueType> Dependencies = new HashSet<ValueType>();
        public HashSet<ValueType> Dependents   = new HashSet<ValueType>();
    }

    private Dictionary<ValueType, Relations> _map = new Dictionary<ValueType, Relations>();

    public void Add(ValueType obj)
    {
        if (!_map.ContainsKey(obj)) _map.Add(obj, new Relations());
    }

    public void Add(ValueType obj, ValueType dependency)
    {
        // if (dependency.Equals(obj)) return;

        if (!_map.ContainsKey(dependency)) _map.Add(dependency, new Relations());

        var dependents = _map[dependency].Dependents;

        if (!dependents.Contains(obj))
        {
            dependents.Add(obj);

            if (!_map.ContainsKey(obj)) _map.Add(obj, new Relations());

            _map[obj].Dependencies.Add(dependency);
        }
    }

    public void Add(ValueType obj, List<ValueType> dependencies)
    {
        Add(obj);
        foreach (var dependency in dependencies) Add(obj, dependency);
    }

    public void Add(ValueType obj, params ValueType[] dependencies)
    {
        Add(obj, dependencies.ToList());
    }

    public bool NodeExists(ValueType obj)
        => _map.ContainsKey(obj);

    public int? GetDependentCount(ValueType obj)
    {
        if (_map.ContainsKey(obj))
        {
            return _map[obj].Dependents.Count;
        }

        return null;
    }

    public ValueType[] GetDependents(ValueType obj)
    {
        if (_map.ContainsKey(obj))
        {
            return _map[obj].Dependents.ToArray();
        }

        return new ValueType[0];
    }

    public int? GetDependencyCount(ValueType obj)
    {
        if (_map.ContainsKey(obj))
        {
            return _map[obj].Dependencies.Count;
        }

        return null;
    }

    public ValueType[] GetDependencies(ValueType obj)
    {
        if (_map.ContainsKey(obj))
        {
            return _map[obj].Dependencies.ToArray();
        }

        return new ValueType[0];
    }

    public void RemoveDependency(ValueType obj, ValueType dependency)
    {
        if (_map.ContainsKey(obj) && _map[obj].Dependencies.Contains(dependency))
        {
            _map[obj].Dependencies.Remove(dependency);
            _map[dependency].Dependents.Remove(obj);
        }
    }

    public void RemoveAllDependencies(ValueType obj)
    {
        foreach (var dependency in _map.TryGetValue(obj, out var relation)
            ? relation.Dependencies.ToArray()
            : [])
        {
            RemoveDependency(obj, dependency);
        }
    }

    public void RemoveDependent(ValueType obj, ValueType dependent)
    {
        if (_map.ContainsKey(obj) && _map[obj].Dependents.Contains(dependent))
        {
            _map[obj].Dependents.Remove(dependent);
            _map[dependent].Dependencies.Remove(obj);
        }
    }

    public void RemoveAllDependents(ValueType obj)
    {
        foreach (var dependent in GetDependents(obj))
        {
            RemoveDependent(obj, dependent);
        }
    }

    /// <summary>
    /// As far as I understand, the return argument is as follows:
    /// 1. The first tuple element returns successfully sorted elements.
    /// 2. The second element contains things that could not be resolved.
    /// </summary>
    public Tuple<List<ValueType>, List<ValueType>> Sort()
    {
        List<ValueType> sorted = new List<ValueType>(), cycled = new List<ValueType>();
        var map = _map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var dependenciesMap = map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Dependencies.Count);

        sorted
            .AddRange(dependenciesMap.Where(kvp => kvp.Value == 0)
            .Select(kvp => kvp.Key));

        for (int idx = 0; idx < sorted.Count; ++idx)
        {
            sorted.AddRange(map[sorted[idx]].Dependents.Where(k => --dependenciesMap[k] == 0));
        }

        cycled.AddRange(dependenciesMap.Where(kvp => kvp.Value != 0).Select(kvp => kvp.Key));

        return new Tuple<List<ValueType>, List<ValueType>>(sorted, cycled);
    }

    public void Clear()
    {
        _map.Clear();
    }
}
