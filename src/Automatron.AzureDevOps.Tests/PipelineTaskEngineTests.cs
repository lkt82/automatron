using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace Automatron.AzureDevOps.Tests;

public class PipelineTaskEngineTests
{
    private readonly ServiceCollection _serviceCollection = new();

    public PipelineTaskEngineTests()
    {
        _serviceCollection.AddSingleton(_ => Substitute.For<IAnsiConsole>());
        _serviceCollection.AddSingleton<IActionRunner, ActionRunner>();
        _serviceCollection.AddSingleton<ITaskModelFactory, TaskModelFactory>();
        _serviceCollection.AddSingleton<TaskEngine>();
        _serviceCollection.AddTransient<TaskVisitor,PipelineTaskVisitor>();
        _serviceCollection.AddSingleton<Func<TaskVisitor>>(c => c.GetRequiredService<TaskVisitor>);
        _serviceCollection.AddSingleton(c => c.GetRequiredService<ITaskModelFactory>().Create());
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_pipeline()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(IntegrationTestingA) });

        var continuousDeployment = Substitute.For<IntegrationTestingA>();

        _serviceCollection.AddTransient(_ => continuousDeployment);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Int");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_inherited_pipeline()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(ContinuousDeployment) });

        var continuousDeployment = Substitute.For<ContinuousDeployment>();

        _serviceCollection.AddTransient(_ => continuousDeployment);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Ci");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_inherited_pipeline_nested_stage()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(ContinuousDeployment), typeof(PulumiContinuousDeploymentPipeline.DeployToTesting) });

        var continuousDeployment = Substitute.For<ContinuousDeployment>();
        var deployToTesting = Substitute.For<PulumiContinuousDeploymentPipeline.DeployToTesting>();

        _serviceCollection.AddTransient(_ => continuousDeployment);
        _serviceCollection.AddTransient(_ => deployToTesting);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Ci-DeployToTesting");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_inherited_pipeline_nested_inherited_stage_nested_inherited_job()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[]
        {
            typeof(ContinuousDeployment),
            typeof(PulumiContinuousDeploymentPipeline.DeployToTesting),
            typeof(PulumiDeploymentStage.DeploymentJob)
        });

        var continuousDeployment = Substitute.For<ContinuousDeployment>();
        var deployToTesting = Substitute.For<PulumiContinuousDeploymentPipeline.DeployToTesting>();
        var deploymentJob = Substitute.For<PulumiDeploymentStage.DeploymentJob>();

        _serviceCollection.AddTransient(_ => continuousDeployment);
        _serviceCollection.AddTransient(_ => deployToTesting);
        _serviceCollection.AddTransient(_ => deploymentJob);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Ci-DeployToTesting-DeploymentJob");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_pipeline_nested_stage()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[]
        {
            typeof(IntegrationTestingA),
            typeof(IntegrationTestingA.IntegrationStage)
        });

        var integrationTestingA = Substitute.For<IntegrationTestingA>();
        var integrationStage = Substitute.For<IntegrationTestingA.IntegrationStage>();

        _serviceCollection.AddTransient(_ => integrationTestingA);
        _serviceCollection.AddTransient(_ => integrationStage);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Int-Integration");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_pipeline_nested_job()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[]
        {
            typeof(IntegrationTestingB),
            typeof(IntegrationTestingB.SetupJob)
        });

        var integrationTestingB = Substitute.For<IntegrationTestingB>();
        var setupJob = Substitute.For<IntegrationTestingB.SetupJob>();

        _serviceCollection.AddTransient(_ => integrationTestingB);
        _serviceCollection.AddTransient(_ => setupJob);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("Int-Setup");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_pipeline_step()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[]
        {
            typeof(IntegrationTestingC)
        });

        var integrationTestingC = Substitute.For<IntegrationTestingC>();

        _serviceCollection.AddTransient(_ => integrationTestingC);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        await taskEngine.RunAsync("Int-Init");

        // Assert
        integrationTestingC.Received().Init();
    }
}