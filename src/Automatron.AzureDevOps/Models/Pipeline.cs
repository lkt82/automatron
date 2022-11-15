#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatron.AzureDevOps.Models
{
    public class Pipeline :IComparer<Stage>, IEqualityComparer<Pipeline>
    {
        public Pipeline(string name, string ymlName, string ymlDir, IEnumerable<Stage> stages, Type type)
        {
            Name = name;
            YmlName = ymlName;
            YmlDir = ymlDir;
            Type = type;

            Stages = CreateStages(stages.ToArray());
        }

        public Pipeline(string name, string ymlName, string ymlDir, Func<Pipeline,IEnumerable<Stage>> stagesFunc,Type type)
        {
            Name = name;
            YmlName = ymlName;
            YmlDir = ymlDir;
            Type = type;

            Stages = CreateStages(stagesFunc(this).ToArray());
        }

        private ISet<Stage> CreateStages(IReadOnlyList<Stage> stages)
        {
            for (var i = 1; i < stages.Count; i++)
            {
                if (stages[i].DependsOn.Any())
                {
                    continue;
                }

                stages[i].DependsOn.Add(stages[i - 1]);
            }

            return new SortedSet<Stage>(stages, this);
        }

        public string Name { get; }

        public string YmlName { get; }

        public string YmlDir { get; }

        public string? DisplayName { get; set; }

        public Type Type { get; }

        public ISet<Stage> Stages { get; }

        public ISet<Variable> Variables { get; } = new HashSet<Variable>();

        public ISet<Parameter> Parameters { get; } = new HashSet<Parameter>();

        public int Compare(Stage? x, Stage? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            if (x.DependsOn.Contains(y))
            {
                return 1;
            }

            if (y.DependsOn.Contains(x))
            {
                return -1;
            }

            return -1;
        }

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