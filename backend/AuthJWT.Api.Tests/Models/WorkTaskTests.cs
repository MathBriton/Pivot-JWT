using AuthJWT.Api.Models;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Models;

public class WorkTaskTests
{
    // --- Status padrão ---

    [Fact]
    public void WorkTask_DefaultStatus_IsPending()
    {
        var task = new WorkTask();

        task.Status.Should().Be(WorkTaskStatus.Pending);
    }

    // --- Transições válidas ---

    [Theory]
    [InlineData(WorkTaskStatus.Pending,    WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Pending,    WorkTaskStatus.Cancelled)]
    [InlineData(WorkTaskStatus.InProgress, WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.InProgress, WorkTaskStatus.Pending)]
    [InlineData(WorkTaskStatus.InProgress, WorkTaskStatus.Cancelled)]
    [InlineData(WorkTaskStatus.Review,     WorkTaskStatus.Completed)]
    [InlineData(WorkTaskStatus.Review,     WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Review,     WorkTaskStatus.Cancelled)]
    public void CanTransitionTo_ValidTransition_ReturnsTrue(string current, string next)
    {
        var task = new WorkTask { Status = current };

        task.CanTransitionTo(next).Should().BeTrue();
    }

    // --- Transições inválidas ---

    [Theory]
    [InlineData(WorkTaskStatus.Completed,  WorkTaskStatus.Pending)]
    [InlineData(WorkTaskStatus.Completed,  WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Completed,  WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.Completed,  WorkTaskStatus.Cancelled)]
    [InlineData(WorkTaskStatus.Cancelled,  WorkTaskStatus.Pending)]
    [InlineData(WorkTaskStatus.Cancelled,  WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Cancelled,  WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.Cancelled,  WorkTaskStatus.Completed)]
    [InlineData(WorkTaskStatus.Pending,    WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.Pending,    WorkTaskStatus.Completed)]
    [InlineData(WorkTaskStatus.InProgress, WorkTaskStatus.Completed)]
    public void CanTransitionTo_InvalidTransition_ReturnsFalse(string current, string next)
    {
        var task = new WorkTask { Status = current };

        task.CanTransitionTo(next).Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_SameStatus_ReturnsFalse()
    {
        var task = new WorkTask { Status = WorkTaskStatus.Pending };

        task.CanTransitionTo(WorkTaskStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void IsClosed_WhenCompleted_ReturnsTrue()
    {
        var task = new WorkTask { Status = WorkTaskStatus.Completed };

        task.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void IsClosed_WhenCancelled_ReturnsTrue()
    {
        var task = new WorkTask { Status = WorkTaskStatus.Cancelled };

        task.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void IsClosed_WhenInProgress_ReturnsFalse()
    {
        var task = new WorkTask { Status = WorkTaskStatus.InProgress };

        task.IsClosed.Should().BeFalse();
    }
}
