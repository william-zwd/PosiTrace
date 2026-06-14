using Microsoft.EntityFrameworkCore;
using PosiTrace.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Retrieve connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext with SQLite
builder.Services.AddDbContextFactory<AppDBContext>(options =>
    options.UseSqlite(connectionString));

// transient Nominatim failure 
builder.Services.AddHttpClient("PosiTraceClient")
    .AddStandardResilienceHandler(options =>
    {
        // Adjust your resilience options here
        options.Retry.MaxRetryAttempts = 3;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
