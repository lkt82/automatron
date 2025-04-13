using System;

namespace Automatron.AzureDevOps.Models;

#if NET8_0
public record PipelineResult(TimeSpan Elapsed)
{
}
#endif