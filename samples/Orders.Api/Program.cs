using Orders.Api;
using Orders.Domain;

var builder = WebApplication.CreateBuilder(args);

// Unexpected failures: one place, one shape (RFC 9457 ProblemDetails).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();

builder.Services.AddSingleton<Store>();
builder.Services.AddSingleton<Orders.Unions.OrderService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOrderEndpoints();

app.Run();
