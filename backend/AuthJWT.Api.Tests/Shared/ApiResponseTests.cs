using AuthJWT.Api.Shared;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Shared;

public class ApiResponseTests
{
    [Fact]
    public void Ok_WithData_ReturnsSuccessTrue()
    {
        var result = ApiResponse<string>.Ok("payload");

        result.Success.Should().BeTrue();
        result.Data.Should().Be("payload");
    }

    [Fact]
    public void Ok_WithCustomMessage_SetsMessage()
    {
        var result = ApiResponse<int>.Ok(42, "Operação realizada com sucesso.");

        result.Message.Should().Be("Operação realizada com sucesso.");
    }

    [Fact]
    public void Ok_WithoutMessage_SetsEmptyMessage()
    {
        var result = ApiResponse<bool>.Ok(true);

        result.Message.Should().BeEmpty();
    }

    [Fact]
    public void Fail_ReturnsSuccessFalse()
    {
        var result = ApiResponse<string>.Fail("Algo deu errado.");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Algo deu errado.");
    }

    [Fact]
    public void Fail_DataIsDefault()
    {
        var result = ApiResponse<string>.Fail("Erro.");

        result.Data.Should().BeNull();
    }

    [Fact]
    public void Fail_WithObjectType_DataIsNull()
    {
        var result = ApiResponse<object>.Fail("Falha na operação.");

        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
    }
}