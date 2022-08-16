using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace Automatron.Tests;

public class TaskEngineTests
{
    private readonly ServiceCollection _serviceCollection = new();

    public TaskEngineTests()
    {
        _serviceCollection.AddSingleton(_ => Substitute.For<IAnsiConsole>());
        _serviceCollection.AddSingleton<IActionRunner,ActionRunner>();
        _serviceCollection.AddSingleton<ITaskModelFactory, TaskModelFactory>();
        _serviceCollection.AddSingleton<TaskEngine>();
        _serviceCollection.AddSingleton(c => c.GetRequiredService<ITaskModelFactory>().Create());
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerA.A_Test));

        // Assert
        result.Should().Be(0);
        taskControllerA.Received().A_Test();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_inherited_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerF1) });

        var taskControllerF = Substitute.For<TaskControllerF1>();

        _serviceCollection.AddTransient(_ => taskControllerF);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerF1)+"-"+nameof(TaskControllerF1.F));

        // Assert
        result.Should().Be(0);
        taskControllerF.Received().F();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_inherited_nested_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerF1),typeof(TaskControllerF2), typeof(TaskControllerFBase.F2) });

        var taskControllerF1 = Substitute.For<TaskControllerF1>();
        var taskControllerF2 = Substitute.For<TaskControllerF1>();
        var taskControllerF2F2 = Substitute.For<TaskControllerFBase.F2>();

        _serviceCollection.AddTransient(_ => taskControllerF1);
        _serviceCollection.AddTransient(_ => taskControllerF2);
        _serviceCollection.AddTransient(_ => taskControllerF2F2);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerF2)+"-"+ nameof(TaskControllerFBase.F2));

        // Assert
        result.Should().Be(0);
        taskControllerF2F2.Received().F2A();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_interface_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerD) });

        var taskControllerD = Substitute.For<ITaskControllerD>();

        _serviceCollection.AddTransient(typeof(TaskControllerD), _ => taskControllerD);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(ITaskControllerD.D_A));

        // Assert
        result.Should().Be(0);
        taskControllerD.Received().D_A();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_default_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddSingleton(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync();

        // Assert
        result.Should().Be(0);
        taskControllerA.Received().Default();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_dependent_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerA.A_B));

        // Assert
        result.Should().Be(0);
        taskControllerA.Received().A();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_dependent_tasks_in_order()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerA.A_C));

        // Assert
        result.Should().Be(0);
        Received.InOrder(() => {
            taskControllerA.A();
            taskControllerA.A_B();
            taskControllerA.A_C();
        });
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_reverse_dependent_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerA.A_E));

        // Assert
        result.Should().Be(0);
        taskControllerA.Received().A_D();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_skip_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA) });

        var taskControllerA = Substitute.For<TaskControllerA>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(new[] { nameof(TaskControllerA.A_B) }, new[] { nameof(TaskControllerA.A) });

        // Assert
        result.Should().Be(0);
        taskControllerA.DidNotReceive().A();
        taskControllerA.Received().A_B();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_dependent_method_task_from_another_type()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerA), typeof(TaskControllerB), typeof(TaskControllerE) });

        var taskControllerA = Substitute.For<TaskControllerA>();
        var taskControllerB = Substitute.For<TaskControllerB>();
        var taskControllerC = Substitute.For<TaskControllerC>();

        _serviceCollection.AddTransient(_ => taskControllerA);
        _serviceCollection.AddTransient(_ => taskControllerB);
        _serviceCollection.AddTransient(_ => taskControllerC);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerB.B_A));

        // Assert
        result.Should().Be(0);
        taskControllerA.Received().A_E();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_dependent_class_task_from_another_type()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerE), typeof(TaskControllerB), typeof(TaskControllerA) });

        var taskControllerE = Substitute.For<TaskControllerE>();
        var taskControllerB = Substitute.For<TaskControllerB>();

        _serviceCollection.AddTransient(_ => taskControllerE);
        _serviceCollection.AddTransient(_ => taskControllerB);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerB.B_B));

        // Assert
        result.Should().Be(0);
        taskControllerE.Received().Default();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_nested_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerC.H.H2), typeof(TaskControllerC), typeof(TaskControllerC.H) });

        var taskControllerHh2 = Substitute.For<TaskControllerC.H.H2>();

        _serviceCollection.AddTransient(_ => taskControllerHh2);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync("H-H2-A");

        // Assert
        result.Should().Be(0);
        taskControllerHh2.Received().A();
    }

    [Fact]
    public async System.Threading.Tasks.Task Should_be_able_to_run_shallow_task()
    {
        // Arrange
        var typeProvider = Substitute.For<ITypeProvider>();
        typeProvider.Types.Returns(new[] { typeof(TaskControllerC.H.H2),typeof(TaskControllerC.H), typeof(TaskControllerC) });

        var taskControllerH = Substitute.For<TaskControllerC.H>();
        var taskControllerHh2 = Substitute.For<TaskControllerC.H.H2>();

        _serviceCollection.AddTransient(_ => taskControllerH);
        _serviceCollection.AddTransient(_ => taskControllerHh2);
        _serviceCollection.AddSingleton(typeProvider);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        var taskEngine = serviceProvider.GetRequiredService<TaskEngine>();

        // Act
        var result = await taskEngine.RunAsync(nameof(TaskControllerC.H));

        // Assert
        result.Should().Be(0);
        taskControllerHh2.Received().A();
    }
}