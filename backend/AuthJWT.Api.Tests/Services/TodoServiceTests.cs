using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _todoRepo;
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        _todoRepo = new Mock<ITodoRepository>();
        _sut = new TodoService(_todoRepo.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsMappedResponsesForUser()
    {
        _todoRepo.Setup(r => r.GetAllByUserAsync(1)).ReturnsAsync(
        [
            new TodoItem { Id = 1, Title = "Tarefa A", Description = "Desc A", UserId = 1 },
            new TodoItem { Id = 2, Title = "Tarefa B", Description = "Desc B", UserId = 1, IsCompleted = true }
        ]);

        var result = (await _sut.GetAllAsync(1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Tarefa A", result[0].Title);
        Assert.True(result[1].IsCompleted);
    }

    [Fact]
    public async Task GetAll_WhenNoTodos_ReturnsEmptyList()
    {
        _todoRepo.Setup(r => r.GetAllByUserAsync(99)).ReturnsAsync([]);

        var result = await _sut.GetAllAsync(99);

        Assert.Empty(result);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_WhenExists_ReturnsMappedResponse()
    {
        _todoRepo.Setup(r => r.GetByIdAsync(5, 1)).ReturnsAsync(
            new TodoItem { Id = 5, Title = "Tarefa X", Description = "Desc X", UserId = 1 });

        var result = await _sut.GetByIdAsync(5, 1);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Tarefa X", result.Title);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNull()
    {
        _todoRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((TodoItem?)null);

        var result = await _sut.GetByIdAsync(999, 1);

        Assert.Null(result);
    }

    // --- Create ---

    [Fact]
    public async Task Create_PersistsNewTodoWithCorrectFields()
    {
        TodoItem? saved = null;
        _todoRepo.Setup(r => r.CreateAsync(It.IsAny<TodoItem>()))
                 .Callback<TodoItem>(t => saved = t)
                 .ReturnsAsync((TodoItem t) => { t.Id = 10; return t; });

        var result = await _sut.CreateAsync(new CreateTodoRequest("Nova Tarefa", "Minha descrição"), userId: 3);

        Assert.NotNull(saved);
        Assert.Equal("Nova Tarefa", saved.Title);
        Assert.Equal("Minha descrição", saved.Description);
        Assert.Equal(3, saved.UserId);
        Assert.False(saved.IsCompleted);
        Assert.Equal(10, result.Id);
    }

    // --- Update ---

    [Fact]
    public async Task Update_WhenExists_ChangesFieldsAndPersists()
    {
        var existing = new TodoItem { Id = 1, Title = "Antigo", Description = "Old", IsCompleted = false, UserId = 1 };
        _todoRepo.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync(existing);
        _todoRepo.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        var result = await _sut.UpdateAsync(1, new UpdateTodoRequest("Novo", "New", IsCompleted: true), userId: 1);

        Assert.NotNull(result);
        Assert.Equal("Novo", result.Title);
        Assert.True(result.IsCompleted);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task Update_WhenCompletedSetToFalse_ClearsCompletedAt()
    {
        var existing = new TodoItem { Id = 2, Title = "T", IsCompleted = true, CompletedAt = DateTime.UtcNow, UserId = 1 };
        _todoRepo.Setup(r => r.GetByIdAsync(2, 1)).ReturnsAsync(existing);
        _todoRepo.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        var result = await _sut.UpdateAsync(2, new UpdateTodoRequest("T", "", IsCompleted: false), userId: 1);

        Assert.False(result!.IsCompleted);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNull()
    {
        _todoRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((TodoItem?)null);

        var result = await _sut.UpdateAsync(999, new UpdateTodoRequest("X", "", false), userId: 1);

        Assert.Null(result);
        _todoRepo.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_WhenExists_ReturnsTrue()
    {
        _todoRepo.Setup(r => r.DeleteAsync(1, 1)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(1, userId: 1);

        Assert.True(result);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsFalse()
    {
        _todoRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(999, userId: 1);

        Assert.False(result);
    }
}
