using AuthJWT.Api.Models;
using FluentAssertions;

namespace AuthJWT.Api.Tests.Models;

public class CustomerTests
{
    [Fact]
    public void Customer_DefaultStatus_IsActive()
    {
        var customer = new Customer();

        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void Customer_DefaultIsDeleted_IsFalse()
    {
        var customer = new Customer();

        customer.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Customer_CreatedAt_IsSetOnCreation()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var customer = new Customer();

        customer.CreatedAt.Should().BeAfter(before);
    }
}
