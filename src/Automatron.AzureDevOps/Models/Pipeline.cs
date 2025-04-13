#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Collections;

namespace Automatron.AzureDevOps.Models
{
    public class Pipeline : IEqualityComparer<Pipeline>
    {
        public Pipeline(string name, IEnumerable<Stage> stages, Type type) :this(name, _ => stages, type)
        {
        }

        public Pipeline(string name, Func<Pipeline,IEnumerable<Stage>> stagesFunc,Type type)
        {
            Name = name;
            Type = type;

            Stages = CreateStages(stagesFunc(this).ToArray());
        }

        private static IEnumerable<Stage> CreateStages(IEnumerable<Stage> stages)
        {
            return new HashSet<Stage>(stages.TopologicalSort(x => x.DependsOn));
        }

        public string Name { get; }

        public string? YmlName { get; set; }

        public string? YmlDir { get; set; }

        public string? RootDir { get; set; }

        public string? DisplayName { get; set; }

        public Type Type { get; }

        public IEnumerable<Stage> Stages { get; }

        public ISet<Variable> Variables { get; } = new HashSet<Variable>();

        public ISet<Parameter> Parameters { get; } = new HashSet<Parameter>();

        public bool Equals(Pipeline? x, Pipeline? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(Pipeline obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
#endif