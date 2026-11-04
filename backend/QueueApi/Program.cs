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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
