using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using FluentAssertions;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class WorkTaskServiceTests
{
    private readonly Mock<IWorkTaskRepository> _repo;
    private readonly WorkTaskService _sut;

    public WorkTaskServiceTests()
    {
        _repo = new Mock<IWorkTaskRepository>();
        _sut = new WorkTaskService(_repo.Object);
    }

    // --- Create ---

    [Fact]
    public async Task Create_WithValidData_ReturnsWorkTaskResponse()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<WorkTask>()))
             .ReturnsAsync((WorkTask t) => { t.Id = 1; return t; });

        var request = new CreateWorkTaskRequest("Implementar login", "Criar endpoint JWT", null, null);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Implementar login");
        result.Status.Should().Be(WorkTaskStatus.Pending);
    }

    [Fact]
    public async Task Create_SetsStatusToPending_ByDefault()
    {
        WorkTask? saved = null;
        _repo.Setup(r => r.CreateAsync(It.IsAny<WorkTask>()))
             .Callback<WorkTask>(t => saved = t)
             .ReturnsAsync((WorkTask t) => t);

        await _sut.CreateAsync(new CreateWorkTaskRequest("Título", null, null, null));

        saved!.Status.Should().Be(WorkTaskStatus.Pending);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_WithExistingTask_ReturnsResponse()
    {
        var task = new WorkTask { Id = 1, Title = "Tarefa", Status = WorkTaskStatus.Pending };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkTask?)null);

        var result = await _sut.GetByIdAsync(99);

        result.Should().BeNull();
    }

    // --- Update ---

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedTask()
    {
        var task = new WorkTask { Id = 1, Title = "Antes", Status = WorkTaskStatus.Pending };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<WorkTask>())).ReturnsAsync((WorkTask t) => t);

        var request = new UpdateWorkTaskRequest("Depois", "Descrição nova", null, null);

        var result = await _sut.UpdateAsync(1, request);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Depois");
    }

    [Fact]
    public async Task Update_WithNonExistentTask_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkTask?)null);

        var result = await _sut.UpdateAsync(99, new UpdateWorkTaskRequest("X", null, null, null));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_WithClosedTask_ReturnsNull()
    {
        var task = new WorkTask { Id = 1, Title = "Feita", Status = WorkTaskStatus.Completed };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var result = await _sut.UpdateAsync(1, new UpdateWorkTaskRequest("X", null, null, null));

        result.Should().BeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<WorkTask>()), Times.Never);
    }

    // --- ChangeStatus ---

    [Fact]
    public async Task ChangeStatus_WithValidTransition_ReturnsUpdatedTask()
    {
        var task = new WorkTask { Id = 1, Title = "Tarefa", Status = WorkTaskStatus.Pending };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<WorkTask>())).ReturnsAsync((WorkTask t) => t);

        var result = await _sut.ChangeStatusAsync(1, new ChangeWorkTaskStatusRequest(WorkTaskStatus.InProgress));

        result.Should().NotBeNull();
        result!.Status.Should().Be(WorkTaskStatus.InProgress);
    }

    [Fact]
    public async Task ChangeStatus_WithInvalidTransition_ReturnsNull()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.Pending };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var result = await _sut.ChangeStatusAsync(1, new ChangeWorkTaskStatusRequest(WorkTaskStatus.Completed));

        result.Should().BeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<WorkTask>()), Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_WithCompletedTask_ReturnsNull()
    {
        var task = new WorkTask { Id = 1, Status = WorkTaskStatus.Completed };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

        var result = await _sut.ChangeStatusAsync(1, new ChangeWorkTaskStatusRequest(WorkTaskStatus.Pending));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangeStatus_WithNonExistentTask_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((WorkTask?)null);

        var result = await _sut.ChangeStatusAsync(99, new ChangeWorkTaskStatusRequest(WorkTaskStatus.InProgress));

        result.Should().BeNull();
    }
}
