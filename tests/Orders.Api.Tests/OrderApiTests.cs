using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Orders.Domain;

namespace Orders.Api.Tests;

public sealed class OrderApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _production =
        factory.WithWebHostBuilder(b => b.UseEnvironment("Production"));

    [Theory]
    [InlineData("alice", "BOOK", 2, HttpStatusCode.OK)]
    [InlineData("alice", "BOOK", 0, HttpStatusCode.BadRequest)]
    [InlineData("zed", "BOOK", 1, HttpStatusCode.NotFound)]
    [InlineData("alice", "PEN", 1, HttpStatusCode.NotFound)]
    [InlineData("bob", "BOOK", 1, HttpStatusCode.Forbidden)]
    [InlineData("alice", "LAPTOP", 5, HttpStatusCode.Conflict)]
    [InlineData("carol", "LAPTOP", 1, HttpStatusCode.PaymentRequired)]
    public async Task EveryOutcome_MapsToItsOwnStatus(string customer, string sku, int quantity, HttpStatusCode expected)
    {
        using var client = _production.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/orders", new PlaceOrderRequest(customer, sku, quantity), TestContext.Current.CancellationToken);

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_Returns500ProblemDetails_WithoutLeakingDetails()
    {
        using var client = _production.CreateClient();

        using var response = await client.GetAsync("/boom", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("An unexpected error occurred.", problem?.Title);
        Assert.DoesNotContain("secret", body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
    }
}
