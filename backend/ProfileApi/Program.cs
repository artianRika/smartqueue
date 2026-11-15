using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProfileApi.Data;
using ProfileApi.Repositories;
using ProfileApi.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// CORS
const string FrontendCorsPolicy = "Frontend";

var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Clerk JWT authentication
var clerkAuthority = builder.Configuration["Clerk:Authority"];

if (string.IsNullOrWhiteSpace(clerkAuthority))
    throw new InvalidOperationException(
        "Clerk:Authority is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = clerkAuthority;
        options.RequireHttpsMetadata = true;

        options.MapInboundClaims = false;
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine(
                    $"Authorization header present: " +
                    $"{context.Request.Headers.ContainsKey("Authorization")}"
                );

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT VALIDATED SUCCESSFULLY!");

                Console.WriteLine(
                    $"Clerk User ID: {context.Principal?.FindFirst("sub")?.Value}"
                );

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT FAILED: {context.Exception}");

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine(
                    $"JWT CHALLENGE: {context.Error} - {context.ErrorDescription}"
                );

                return Task.CompletedTask;
            }
        };

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

// Swagger
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

// PostgreSQL
builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ProfileDb"),
        npgsql => npgsql.MigrationsHistoryTable(
            "__EFMigrationsHistory",
            "profile"
        )
    )
);

// Repositories
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<
    IOrganizationMembershipRepository,
    OrganizationMembershipRepository>();

var app = builder.Build();

// Seeder
if (args.Contains("--seed"))
{
    if (!app.Environment.IsDevelopment())
    {
        Console.Error.WriteLine(
            "Seeding is only available in Development.");

        Environment.ExitCode = 1;
        return;
    }

    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ProfileDbContext>();

    await ProfileDbSeeder.SeedAsync(db);
    return;
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();