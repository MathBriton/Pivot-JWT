using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class UpdateCustomerRequestValidatorTests
{
    private readonly UpdateCustomerRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var request = new UpdateCustomerRequest("João Silva", "joao@email.com", "11999999999", "Empresa X", CustomerStatus.Active, null);

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyName_FailsOnName(string name)
    {
        var request = new UpdateCustomerRequest(name, "joao@email.com", null, null, CustomerStatus.Active, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("sem@dominio")]
    public void Validate_WithInvalidEmail_FailsOnEmail(string email)
    {
        var request = new UpdateCustomerRequest("João", email, null, null, CustomerStatus.Active, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("StatusInvalido")]
    [InlineData("active")]
    public void Validate_WithInvalidStatus_FailsOnStatus(string status)
    {
        var request = new UpdateCustomerRequest("João", "joao@email.com", null, null, status, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Status);
    }

    [Fact]
    public void Validate_WithInactiveStatus_Passes()
    {
        var request = new UpdateCustomerRequest("João", "joao@email.com", null, null, CustomerStatus.Inactive, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Status);
    }
}
