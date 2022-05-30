using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    //public class AutomatronTask : Script
    //{
    //    public AutomatronTask(IJob job,string[] targets, bool skipDependencies=false, bool parallel=false) : base(job,$"dotnet run -- {Arguments(targets, skipDependencies, parallel)}")
    //    {
    //    }

    //    private static string Arguments(string[] targets, bool skipDependencies, bool parallel)
    //    {
    //        var arguments = string.Join(" ", targets);

    //        if (skipDependencies)
    //        {
    //            arguments += " -s";
    //        }
    //        if (parallel)
    //        {
    //            arguments += " -p";
    //        }

    //        return arguments;
    //    }
    //}

    public class AutomatronTask : DotNetTask
    {

        public AutomatronTask(IJob job, string[] targets, bool skipDependencies = false, bool parallel = false) : base(job,new DotNetTaskInputs("run") { Arguments = Arguments(targets, skipDependencies, parallel) })
        {
        }

        private static string Arguments(string[] targets, bool skipDependencies, bool parallel)
        {
            var arguments = string.Join(" ", targets);

            if (skipDependencies)
            {
                arguments += " -s";
            }
            if (parallel)
            {
                arguments += " -p";
            }

            return arguments;
        }

        [YamlIgnore]
        public string? WorkingDirectory
        {
            get => Inputs!.WorkingDirectory;
            set => Inputs!.WorkingDirectory = value;
        }

        [YamlIgnore]
        public string[]? Secrets { get; set; }

        //public IDictionary<string, object>? Env
        //{
        //    get
        //    {
        //        return Secrets?.ToDictionary(c => c, c => (object)$"$({c})") ?? _env;
        //    }
        //    set => _env = value;
        //}
    }
}
