using AuthJWT.Api.DTOs;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class CreateWorkTaskRequestValidatorTests
{
    private readonly CreateWorkTaskRequestValidator _sut = new();

    [Fact]
    public void Validate_WithValidData_Passes()
    {
        var request = new CreateWorkTaskRequest("Implementar login", "Descrição", null, DateTime.UtcNow.AddDays(3));

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithoutOptionalFields_Passes()
    {
        var request = new CreateWorkTaskRequest("Implementar login", null, null, null);

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithEmptyTitle_FailsOnTitle(string title)
    {
        var request = new CreateWorkTaskRequest(title, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Title);
    }

    [Fact]
    public void Validate_WithTitleTooLong_FailsOnTitle()
    {
        var longTitle = new string('A', 301);
        var request = new CreateWorkTaskRequest(longTitle, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Title);
    }

    [Fact]
    public void Validate_WithPastDeadline_FailsOnDeadline()
    {
        var request = new CreateWorkTaskRequest("Título", null, null, DateTime.UtcNow.AddDays(-1));

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Deadline);
    }
}
