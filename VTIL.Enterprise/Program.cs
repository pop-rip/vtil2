/*
 * VTIL.Enterprise - Main Program
 * Copyright (c) 2025 VTIL2 Project
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using VTIL.Enterprise.Compliance.Services;
using VTIL.Enterprise.Services;
using VTIL.Enterprise.Monitoring;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/vtil-enterprise-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VTIL.Enterprise API",
        Version = "v1",
        Description = "Enterprise-grade binary deobfuscation and compliance management API",
        Contact = new OpenApiContact
        {
            Name = "VTIL2 Team",
            Email = "support@vtil2.com",
            Url = new Uri("https://github.com/vtil2/vtil2")
        },
        License = new OpenApiLicense
        {
            Name = "BSD 3-Clause License",
            Url = new Uri("https://opensource.org/licenses/BSD-3-Clause")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register services
builder.Services.AddSingleton<IComplianceService, ComplianceService>();
builder.Services.AddSingleton<IVTILAnalysisService, VTILAnalysisService>();
builder.Services.AddSingleton<IAuditService, AuditService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<ComplianceHealthCheck>("compliance", tags: new[] { "compliance", "ready" })
    .AddCheck<VTILAnalysisHealthCheck>("analysis", tags: new[] { "analysis", "ready" })
    .AddCheck<SystemResourcesHealthCheck>("resources", tags: new[] { "resources", "live" });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VTIL.Enterprise API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseResponseCompression();

// Use Prometheus metrics
app.UseMetricServer();   // /metrics endpoint
app.UseHttpMetrics();    // Automatic HTTP metrics

app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Welcome message
app.MapGet("/", () => new
{
    service = "VTIL.Enterprise",
    version = "1.0.0",
    description = "Enterprise-grade binary deobfuscation and compliance management",
    documentation = "/swagger",
    health = "/health",
    metrics = "/metrics",
    endpoints = new
    {
        compliance = "/api/v1/compliance",
        analysis = "/api/v1/analysis"
    }
});

Log.Information("🚀 VTIL.Enterprise starting...");
Log.Information("📊 Swagger UI available at: https://localhost:5001");
Log.Information("🏥 Health checks available at: https://localhost:5001/health");
Log.Information("📈 Metrics available at: https://localhost:5001/metrics");

app.Run();

Log.Information("👋 VTIL.Enterprise shutting down...");
Log.CloseAndFlush();

