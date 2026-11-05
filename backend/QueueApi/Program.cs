using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.Infrastructure;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Before HTTPS redirection so preflight requests are answered instead of redirected
app.UseCors(FrontendCorsPolicy);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
