using Microsoft.EntityFrameworkCore;
using ProfileApi.Data;
using ProfileApi.Repositories;
using ProfileApi.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    await db.Database.MigrateAsync();
}

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map controller endpoints
app.MapControllers();

app.Run();