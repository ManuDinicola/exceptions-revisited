using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task UnexpectedException_IsLoggedExactlyOnce()
    {
        var logs = new CapturingLoggerProvider();
        using var app = factory.WithWebHostBuilder(b => b
            .UseEnvironment("Production")
            .ConfigureLogging(l => l.AddProvider(logs)));
        using var client = app.CreateClient();

        using var response = await client.GetAsync("/boom", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errors = logs.Entries.Where(e => e.Level >= LogLevel.Error).ToList();
        Assert.Single(errors);
        Assert.Equal("Orders.Api.UnexpectedExceptionHandler", errors[0].Category);
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public List<(string Category, LogLevel Level)> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new Capture(categoryName, Entries);

        public void Dispose() { }

        private sealed class Capture(string category, List<(string, LogLevel)> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                lock (entries) entries.Add((category, logLevel));
            }
        }
    }
}
