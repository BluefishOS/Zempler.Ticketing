using Carter;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Zempler.Ticketing.Common.Exceptions;
using Zempler.Ticketing.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ticketing.db"));

// Add Carter for modular routing
builder.Services.AddCarter();

// Add ProblemDetails & Custom Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add OpenAPI / Scalar support
builder.Services.AddOpenApi();

// Add CORS (for frontend Next.js application)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// Configure HTTP Request Pipeline
app.UseExceptionHandler(); // Invokes GlobalExceptionHandler -> returns ProblemDetails

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI JSON endpoint
    app.MapOpenApi();

    // Enable Scalar API Reference UI at /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Zempler Ticketing API")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11);
    });

    // Auto-create SQLite database and seed initial test data
    await DbInitializer.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

// Map Carter Endpoints
app.MapCarter();



app.UseSerilogRequestLogging();

app.Run();



public partial class Program { }