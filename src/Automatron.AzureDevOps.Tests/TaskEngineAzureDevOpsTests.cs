using FluentAssertions;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace Automatron.AzureDevOps.Tests;

public class TaskEngineAzureDevOpsTests
{
    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_pipeline()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(IntegrationTestingA)
        });

        var job = Substitute.For<IntegrationTestingA.IntegrationStage.SetupJob>();

        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(Arg.Any<Type>()).Returns(job);

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(ContinuousDeployment)
        });

        var serviceProvider = Substitute.For<IServiceProvider>();

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(ContinuousDeployment),
            typeof(PulumiContinuousDeploymentPipeline.DeployToTesting)
        });

        var serviceProvider = Substitute.For<IServiceProvider>();

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(ContinuousDeployment),
            typeof(ContinuousDeployment2),
            typeof(PulumiContinuousDeploymentPipeline.DeployToTesting),
            typeof(PulumiDeploymentStage.DeploymentJob)
        });

        var serviceProvider = Substitute.For<IServiceProvider>();

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(IntegrationTestingA),
            typeof(IntegrationTestingA.IntegrationStage)
        });

        var job = Substitute.For<IntegrationTestingA.IntegrationStage.SetupJob>();

        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(Arg.Any<Type>()).Returns(job);

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(IntegrationTestingB),
            typeof(IntegrationTestingB.SetupJob)
        });

        var job = Substitute.For<IntegrationTestingB.SetupJob>();

        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(Arg.Any<Type>()).Returns(job);

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

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
        typeProvider.GetTypes().Returns(new[]
        {
            typeof(IntegrationTestingC)
        });

        var job = Substitute.For<IntegrationTestingC>();

        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(Arg.Any<Type>()).Returns(job);

        var taskBuilder = new AzureDevOpsTaskModelFactoryDecorator(Substitute.For<ITaskModelFactory>(), serviceProvider, typeProvider);

        var taskEngine = new TaskEngine(Substitute.For<IAnsiConsole>(), taskBuilder.Create());

        // Act
        await taskEngine.RunAsync("Int-Init");

        // Assert
        job.Received().Init();
    }
}