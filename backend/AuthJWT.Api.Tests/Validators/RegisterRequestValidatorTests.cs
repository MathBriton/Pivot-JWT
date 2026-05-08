using AuthJWT.Api.DTOs;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidData_PassesAllRules()
    {
        var request = new RegisterRequest("Ana Silva", "ana@email.com", "senha123");

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyName_FailsOnName(string name)
    {
        var request = new RegisterRequest(name, "ana@email.com", "senha123");

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("nao-e-email")]
    [InlineData("semdominio@")]
    [InlineData("@semusuario.com")]
    [InlineData("")]
    public void Validate_WithInvalidEmail_FailsOnEmail(string email)
    {
        var request = new RegisterRequest("Ana", email, "senha123");

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12345")]
    public void Validate_WithShortOrEmptyPassword_FailsOnPassword(string password)
    {
        var request = new RegisterRequest("Ana", "ana@email.com", password);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Password);
    }

    [Fact]
    public void Validate_PasswordWithExactlyMinLength_Passes()
    {
        var request = new RegisterRequest("Ana", "ana@email.com", "abc123");

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Password);
    }
}