using TicketFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Interfaces;
using TicketFlow.Infrastructure.Services;
using StackExchange.Redis;
using TicketFlow.Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IMessageBusService, RabbitMqService>();
builder.Services.AddHostedService<EmailWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
