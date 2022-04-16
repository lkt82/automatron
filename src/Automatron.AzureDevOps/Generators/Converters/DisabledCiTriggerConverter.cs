using System;
using Automatron.AzureDevOps.Generators.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Converters
{
    internal sealed class DisabledCiTriggerConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(DisabledCiTrigger);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            emitter.Emit(new Scalar(null, "none"));
        }
    }
}