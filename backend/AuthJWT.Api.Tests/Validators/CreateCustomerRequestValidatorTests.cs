using AuthJWT.Api.DTOs;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class CreateCustomerRequestValidatorTests
{
    private readonly CreateCustomerRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var request = new CreateCustomerRequest("João Silva", "joao@email.com", "11999999999", "Empresa X", null);

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyName_FailsOnName(string name)
    {
        var request = new CreateCustomerRequest(name, "joao@email.com", null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void Validate_WithNameTooLong_FailsOnName()
    {
        var longName = new string('A', 201);
        var request = new CreateCustomerRequest(longName, "joao@email.com", null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("sem@dominio")]
    public void Validate_WithInvalidEmail_FailsOnEmail(string email)
    {
        var request = new CreateCustomerRequest("João", email, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Fact]
    public void Validate_WithoutPhoneAndCompany_Passes()
    {
        var request = new CreateCustomerRequest("João", "joao@email.com", null, null, null);

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }
}
