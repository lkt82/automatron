#if NET6_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Automatron.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Automatron.AzureDevOps.Models;

public class PipelineEngine : IPipelineEngine
{
    private readonly IServiceProvider _serviceProvider;

    public PipelineEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public event EventHandler<PipelineModelCompletedArgs<Pipeline>>? OnPipelineCompleted;
    public event EventHandler<PipelineModelStartingArgs<Pipeline>>? OnPipelineStarting;
    public event EventHandler<PipelineModelFailedArgs<Pipeline>>? OnPipelineFailed;
    public event EventHandler<PipelineModelCompletedArgs<Stage>>? OnStageCompleted;
    public event EventHandler<PipelineModelStartingArgs<Stage>>? OnStageStarting;
    public event EventHandler<PipelineModelCompletedArgs<Job>>? OnJobCompleted;
    public event EventHandler<PipelineModelStartingArgs<Job>>? OnJobStarting;
    public event EventHandler<PipelineModelCompletedArgs<Step>>? OnStepCompleted;
    public event EventHandler<PipelineModelStartingArgs<Step>>? OnStepStarting;
    public event EventHandler<PipelineModelFailedArgs<Step>>? OnStepFailed;
    public event EventHandler<PipelineModelFailedArgs<Job>>? OnJobFailed;
    public event EventHandler<PipelineModelFailedArgs<Stage>>? OnStageFailed;

    private async Task<PipelineResult> RunPipeline(Pipeline pipeline, IEnumerable<VariableValue> variables, IEnumerable<ParameterValue> parameters, Func<IServiceProvider,Stage, Task> stageAction, bool dryRun)
    {
        ConvertVariables(variables, pipeline.Variables.ToDictionary(c => c.Name.ToLower(), c => c));
        ConvertParameters(parameters, pipeline.Parameters.ToDictionary(c => c.Name.ToLower(), c => c));

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            OnPipelineStarting?.Invoke(this, new PipelineModelStartingArgs<Pipeline>(pipeline));

            foreach (var stage in pipeline.Stages)
            {
                await stageAction(_serviceProvider, stage);
            }

            OnPipelineCompleted?.Invoke(this, new PipelineModelCompletedArgs<Pipeline>(pipeline, stopWatch.Elapsed, dryRun));

            return new PipelineResult(stopWatch.Elapsed);

        }
        catch (StageException exception)
        {
            var pipelineException = new PipelineException(pipeline, new[] { exception }, stopWatch.Elapsed);

            OnPipelineFailed?.Invoke(this, new PipelineModelFailedArgs<Pipeline>(pipeline, stopWatch.Elapsed, pipelineException, dryRun));

            throw pipelineException;
        }
        finally
        {
            stopWatch.Stop();
        }
    }

    private async Task RunStage(IServiceProvider serviceProvider, Stage stage, IEnumerable<VariableValue> variables, Func<IServiceProvider, Job, Task> jobAction,bool dryRun)
    {
        ConvertVariables(variables, stage.Variables.ToDictionary(c => c.Name.ToLower(), c => c));
        
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            OnStageStarting?.Invoke(this, new PipelineModelStartingArgs<Stage>(stage));

            foreach (var job in stage.Jobs)
            {
                await jobAction(serviceProvider, job);
            }

            OnStageCompleted?.Invoke(this, new PipelineModelCompletedArgs<Stage>(stage, stopWatch.Elapsed, dryRun));
        }
        catch (JobException exception)
        {
            var stageException = new StageException(stage, new[] { exception }, stopWatch.Elapsed);

            OnStageFailed?.Invoke(this, new PipelineModelFailedArgs<Stage>(stage, stopWatch.Elapsed, stageException, dryRun));

            throw stageException;
        }
        finally
        {
            stopWatch.Stop();
        }
    }

    private async Task RunJob(IServiceProvider serviceProvider, Job job, IEnumerable<VariableValue> variables, Func<Step,object, Task> stepAction,bool dryRun)
    {
        ConvertVariables(variables, job.Variables.ToDictionary(c => c.Name.ToLower(), c => c));
        
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            OnJobStarting?.Invoke(this, new PipelineModelStartingArgs<Job>(job));

            await using var scope = _serviceProvider.CreateAsyncScope();

            var pipelineService = scope.ServiceProvider.GetRequiredService(job.Stage.Pipeline.Type);
            BindProperties(pipelineService, job.Stage.Pipeline.Variables);
            BindProperties(pipelineService, job.Stage.Pipeline.Parameters);

            var stageService = scope.ServiceProvider.GetRequiredService(job.Stage.Type);
            BindProperties(stageService, job.Stage.Variables);

            var jobService = serviceProvider.GetRequiredService(job.Type);
            BindProperties(jobService, job.Variables);

            foreach (var step in job.Steps)
            {
                await stepAction(step, jobService);
            }

            OnJobCompleted?.Invoke(this, new PipelineModelCompletedArgs<Job>(job, stopWatch.Elapsed, dryRun));
        }
        catch (StepException exception)
        {
            var jobException = new JobException(job, exception, stopWatch.Elapsed);

            OnJobFailed?.Invoke(this, new PipelineModelFailedArgs<Job>(job, stopWatch.Elapsed, jobException, dryRun));

            throw jobException;
        }
        finally
        {
            stopWatch.Stop();
        }
    }

    private static Task RunStepAction(Step step, object jobService)
    {
        var result = step.Action.Invoke(jobService);

        if (result is Task asyncResult)
        {
            return asyncResult;
        }

        return Task.CompletedTask;
    }

    private async Task RunStep(Step step,object jobService,bool dryRun)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        try
        {
            OnStepStarting?.Invoke(this, new PipelineModelStartingArgs<Step>(step));

            if (!dryRun)
            {
                await RunStepAction(step, jobService);
            }

            OnStepCompleted?.Invoke(this, new PipelineModelCompletedArgs<Step>(step, stopWatch.Elapsed, dryRun));
        }
        catch (Exception exception)
        {
            var stepException = new StepException(step, stopWatch.Elapsed, exception.GetBaseException());

            OnStepFailed?.Invoke(this, new PipelineModelFailedArgs<Step>(step, stopWatch.Elapsed, stepException, dryRun));

            throw stepException;
        }
        finally
        {
            stopWatch.Stop();
        }
    }

    private static void BindProperties(object service, IEnumerable<IPropertyValue> variables)
    {
        foreach (var variable in variables)
        {
            if (variable.Value == null)
            {
                continue;
            }
            variable.Property.SetValue(service, variable.Value);
        }
    }

    private static void ConvertVariables(IEnumerable<VariableValue> input, IDictionary<string, Variable> variables)
    {
        foreach (var variable in input)
        {
            if (!variables.TryGetValue(variable.Name, out var foundVariable))
            {
                continue;
            }

            var conv = TypeDescriptor.GetConverter(foundVariable.Property.PropertyType);
            foundVariable.Value = conv.ConvertTo(variable.Value, foundVariable.Property.PropertyType);
        }
    }

    private static void ConvertParameters(IEnumerable<ParameterValue> input, IDictionary<string, Parameter> variables)
    {
        foreach (var variable in input)
        {
            if (!variables.TryGetValue(variable.Name, out var foundVariable))
            {
                continue;
            }

            var conv = TypeDescriptor.GetConverter(foundVariable.Property.PropertyType);
            foundVariable.Value = conv.ConvertTo(variable.Value, foundVariable.Property.PropertyType);
        }
    }

    public async Task<PipelineResult> Run(Pipeline pipeline, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters, bool dryRun)
    {
        return await RunPipeline(pipeline, variables ?? Array.Empty<VariableValue>(), parameters ?? Array.Empty<ParameterValue>(), async (serviceProvider, targetStage) =>
        {
            await RunStage(serviceProvider, targetStage, variables ?? Array.Empty<VariableValue>(), async (provider, targetJob) =>
            {
                await RunJob(provider, targetJob, variables ?? Array.Empty<VariableValue>(), async (targetStep, jobService) => await RunStep(targetStep, jobService, dryRun), dryRun);
            }, dryRun);
        }, dryRun);
    }

    public async Task<PipelineResult> Run(Stage stage, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters,bool dryRun)
    {
        return await RunPipeline(stage.Pipeline, variables ?? Array.Empty<VariableValue>(), parameters ?? Array.Empty<ParameterValue>(), async (serviceProvider,targetStage)=>
        {
            if (targetStage.Equals(stage))
            {
                await RunStage(serviceProvider, stage, variables ?? Array.Empty<VariableValue>(), async (provider, targetJob) =>
                {
                    await RunJob(provider, targetJob, variables ?? Array.Empty<VariableValue>(), async (step, o) => await RunStep(step, o, dryRun), dryRun);
                }, dryRun);
            }
        }, dryRun);
    }

    public async Task<PipelineResult> Run(Job job, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters, bool dryRun)
    {
        return await RunPipeline(job.Stage.Pipeline, variables ?? Array.Empty<VariableValue>(), parameters ?? Array.Empty<ParameterValue>(), async (serviceProvider, targetStage) =>
        {
            if (targetStage.Equals(job.Stage))
            {
                await RunStage(serviceProvider, job.Stage, variables ?? Array.Empty<VariableValue>(), async (provider, targetJob) =>
                {
                    if (targetJob.Equals(job))
                    {
                        await RunJob(provider, targetJob, variables ?? Array.Empty<VariableValue>(), async (targetStep, jobService) => await RunStep(targetStep, jobService, dryRun), dryRun);
                    }

                }, dryRun);
            }
        }, dryRun);
    }

    public async Task<PipelineResult> Run(Step step, IEnumerable<VariableValue>? variables, IEnumerable<ParameterValue>? parameters, bool dryRun,bool runDependencies)
    {
        return await RunPipeline(step.Job.Stage.Pipeline, variables ?? Array.Empty<VariableValue>(), parameters ?? Array.Empty<ParameterValue>(), async (serviceProvider, stage) =>
        {
            if (stage.Equals(step.Job.Stage))
            {
                await RunStage(serviceProvider, step.Job.Stage, variables ?? Array.Empty<VariableValue>(), async (provider, job) =>
                {
                    if (job.Equals(step.Job))
                    {
                        var dependencies = new List<Step>();

                        await RunJob(provider, job, variables ?? Array.Empty<VariableValue>(), async (targetStep, jobService) =>
                        {
                            if (targetStep.Equals(step))
                            {
                                if (runDependencies)
                                {
                                    foreach (var dependency in dependencies)
                                    {
                                        await RunStep(dependency, jobService, dryRun);
                                    }
                                }

                                await RunStep(step, jobService, dryRun);
                            }
                            else
                            {
                                dependencies.Add(targetStep);
                            }
                        }, dryRun);
                    }

                }, dryRun);
            }
        }, dryRun);
    }

}
#endif