using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Repositories;
using AuthJWT.Api.Services;
using FluentAssertions;
using Moq;

namespace AuthJWT.Api.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repo;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _sut = new CustomerService(_repo.Object);
    }

    // --- Create ---

    [Fact]
    public async Task Create_WithValidData_ReturnsCustomerResponse()
    {
        _repo.Setup(r => r.EmailExistsAsync("joao@email.com", null)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
             .ReturnsAsync((Customer c) => { c.Id = 1; return c; });

        var request = new CreateCustomerRequest("João Silva", "joao@email.com", "11999999999", "Empresa X", null);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result!.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@email.com");
        result.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_ReturnsNull()
    {
        _repo.Setup(r => r.EmailExistsAsync("existente@email.com", null)).ReturnsAsync(true);

        var request = new CreateCustomerRequest("Alguém", "existente@email.com", null, null, null);

        var result = await _sut.CreateAsync(request);

        result.Should().BeNull();
        _repo.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_WithExistingCustomer_ReturnsCustomerResponse()
    {
        var customer = new Customer { Id = 1, Name = "Ana", Email = "ana@email.com" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Ana");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var result = await _sut.GetByIdAsync(99);

        result.Should().BeNull();
    }

    // --- Update ---

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedCustomer()
    {
        var customer = new Customer { Id = 1, Name = "Ana", Email = "ana@email.com", Status = CustomerStatus.Active };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _repo.Setup(r => r.EmailExistsAsync("ana@email.com", 1)).ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
             .ReturnsAsync((Customer c) => c);

        var request = new UpdateCustomerRequest("Ana Souza", "ana@email.com", "11988887777", "Nova Empresa", CustomerStatus.Active, "Nota atualizada");

        var result = await _sut.UpdateAsync(1, request);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Ana Souza");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var request = new UpdateCustomerRequest("X", "x@email.com", null, null, CustomerStatus.Active, null);

        var result = await _sut.UpdateAsync(99, request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_WithDuplicateEmail_ReturnsNull()
    {
        var customer = new Customer { Id = 1, Name = "Ana", Email = "ana@email.com" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _repo.Setup(r => r.EmailExistsAsync("outro@email.com", 1)).ReturnsAsync(true);

        var request = new UpdateCustomerRequest("Ana", "outro@email.com", null, null, CustomerStatus.Active, null);

        var result = await _sut.UpdateAsync(1, request);

        result.Should().BeNull();
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_WithExistingCustomer_ReturnsTrue()
    {
        var customer = new Customer { Id = 1, Name = "Ana", Email = "ana@email.com" };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _repo.Setup(r => r.SoftDeleteAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);

        var result = await _sut.DeleteAsync(1);

        result.Should().BeTrue();
        _repo.Verify(r => r.SoftDeleteAsync(customer), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var result = await _sut.DeleteAsync(99);

        result.Should().BeFalse();
    }
}
