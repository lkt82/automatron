#if NET8_0
using System;

namespace Automatron.AzureDevOps.Models;

public record PipelineResult(TimeSpan Elapsed);
#endif