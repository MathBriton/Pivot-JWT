using AuthJWT.Api.DTOs;
using AuthJWT.Api.Models;
using AuthJWT.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AuthJWT.Api.Tests.Validators;

public class ChangeWorkTaskStatusRequestValidatorTests
{
    private readonly ChangeWorkTaskStatusRequestValidator _sut = new();

    [Theory]
    [InlineData(WorkTaskStatus.Pending)]
    [InlineData(WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.Completed)]
    [InlineData(WorkTaskStatus.Cancelled)]
    public void Validate_WithValidStatus_Passes(string status)
    {
        var request = new ChangeWorkTaskStatusRequest(status);

        var result = _sut.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("StatusInvalido")]
    [InlineData("pending")]
    [InlineData("in_progress")]
    public void Validate_WithInvalidStatus_FailsOnStatus(string status)
    {
        var request = new ChangeWorkTaskStatusRequest(status);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Status);
    }
}
