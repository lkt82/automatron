#if NET6_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automatron.AzureDevOps.Models;

public interface IPipelineEngine
{
    public event EventHandler<PipelineModelCompletedArgs<Pipeline>> OnPipelineCompleted;
    public event EventHandler<PipelineModelStartingArgs<Pipeline>> OnPipelineStarting;
    public event EventHandler<PipelineModelFailedArgs<Pipeline>> OnPipelineFailed;

    public event EventHandler<PipelineModelCompletedArgs<Stage>> OnStageCompleted;
    public event EventHandler<PipelineModelStartingArgs<Stage>> OnStageStarting;
    public event EventHandler<PipelineModelFailedArgs<Stage>> OnStageFailed;

    public event EventHandler<PipelineModelCompletedArgs<Job>> OnJobCompleted;
    public event EventHandler<PipelineModelStartingArgs<Job>> OnJobStarting;
    public event EventHandler<PipelineModelFailedArgs<Job>> OnJobFailed;

    public event EventHandler<PipelineModelCompletedArgs<Step>> OnStepCompleted;
    public event EventHandler<PipelineModelStartingArgs<Step>> OnStepStarting;
    public event EventHandler<PipelineModelFailedArgs<Step>> OnStepFailed;

    public Task<PipelineResult> Run(Pipeline pipeline, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters);

    public Task<PipelineResult> Run(Stage stage, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters);

    public Task<PipelineResult> Run(Job job, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters);

    public Task<PipelineResult> Run(Step step, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters);

}

#endif