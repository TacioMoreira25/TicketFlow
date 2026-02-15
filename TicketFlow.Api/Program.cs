using TicketFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.Interfaces;
using TicketFlow.Infrastructure.Services;
using StackExchange.Redis;
using TicketFlow.Api.BackgroundServices;
using Serilog;
using TicketFlow.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var seqUrl = builder.Configuration["Seq:Url"] ?? "http://localhost:5341";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() 
    .WriteTo.Seq(seqUrl)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["RedisConnection"] ?? "localhost";
    
    var configuration = ConfigurationOptions.Parse(connectionString, true);
    
    configuration.AbortOnConnectFail = false;
    
    return ConnectionMultiplexer.Connect(configuration);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
        Log.Error(ex, "❌ Ocorreu um erro ao migrar o banco de dados.");
    }
}

app.Run();

public partial class Program {}
