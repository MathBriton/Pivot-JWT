using AuthJWT.Api.DTOs;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidCredentials_Passes()
    {
        var request = new LoginRequest("ana@email.com", "senha123");

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("sem@dominio")]
    public void Validate_WithInvalidEmail_FailsOnEmail(string email)
    {
        var request = new LoginRequest(email, "senha123");

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyPassword_FailsOnPassword(string password)
    {
        var request = new LoginRequest("ana@email.com", password);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Password);
    }
}