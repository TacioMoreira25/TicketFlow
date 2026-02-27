using FluentValidation;
using TicketFlow.Application.Validators;
using TicketFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Interfaces;
using TicketFlow.Infrastructure.Services;
using StackExchange.Redis;
using TicketFlow.Api.BackgroundServices;
using Serilog;
using TicketFlow.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var seqUrl = builder.Configuration["Seq:Url"] ?? "http://localhost:5341";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() 
    .WriteTo.Seq(seqUrl)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = builder.Configuration.GetConnectionString("redis-cache") ?? "";

    if (string.IsNullOrEmpty(redisConnectionString))
    {
        redisConnectionString = "localhost";
    }

    var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
    
    configuration.AbortOnConnectFail = false;
    
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("ticketflow-db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString,new MySqlServerVersion(new Version(8, 0, 30))));

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IMessageBusService, RabbitMqService>();
builder.Services.AddHostedService<EmailWorker>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        context.Database.Migrate();
        
        Log.Information("Banco de dados migrado com sucesso!");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Ocorreu um erro ao migrar o banco de dados.");
    }
}

app.Run();

public partial class Program {}
