using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QueueApi.Data;
using QueueApi.Infrastructure;
using QueueApi.Messaging;
using QueueApi.Repositories;
using QueueApi.Repositories.Interfaces;
using QueueApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ApiExceptionFilter());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

const string FrontendCorsPolicy = "Frontend";

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var clerkAuthority = builder.Configuration["Clerk:Authority"];

if (string.IsNullOrWhiteSpace(clerkAuthority))
    throw new InvalidOperationException("Clerk:Authority is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = clerkAuthority;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = clerkAuthority.TrimEnd('/'),
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Clerk JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddDbContext<QueueDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("QueueDb"),
        npgsql => npgsql.MigrationsHistoryTable(
            "__EFMigrationsHistory",
            "queue"
        )
    )
);

builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<ICounterRepository, CounterRepository>();
builder.Services.AddScoped<ICounterAssignmentRepository, CounterAssignmentRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IQueueDailyCounterRepository, QueueDailyCounterRepository>();
builder.Services.AddScoped<IProfileLookup, ProfileLookup>();

var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];

if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddHostedService<OutboxPublisher>();
}

var app = builder.Build();

if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    app.Logger.LogWarning(
        "ServiceBus:ConnectionString is not set. Outbox events will be stored but not published.");
}

if (args.Contains("--seed"))
{
    if (!app.Environment.IsDevelopment())
    {
        Console.Error.WriteLine("Seeding is only available in Development.");
        Environment.ExitCode = 1;
        return;
    }

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<QueueDbContext>();
    await QueueDbSeeder.SeedAsync(db);
    return;
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QueueDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Before HTTPS redirection so preflight requests are answered instead of redirected
app.UseCors(FrontendCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
