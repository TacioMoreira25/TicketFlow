using TicketFlow.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TicketFlow.Infrastructure.Services;

public class RabbitMqService : IMessageBusService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService(IConfiguration configuration)
    {
        var rabbitConnectionString = configuration.GetConnectionString("rabbitmq-bus") ?? "";
        var user = configuration["RabbitMQ:UserName"] ?? "guest";
        var pass = configuration["RabbitMQ:Password"] ?? "guest";
        
        _factory = new ConnectionFactory
        {
            Uri = new Uri(rabbitConnectionString),
            UserName = user,
            Password = pass
        };
    }

    public async Task PublishAsync<T>(string queue, T message)
    {
        // 1. Cria a conexão com o RabbitMQ
        await using var connection = await _factory.CreateConnectionAsync();
        
        // 2. Cria um canal (é por onde a mensagem trafega)
        await using var channel = await connection.CreateChannelAsync();

        // 3. Declara a Fila (Garante que ela existe)
        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // 4. Prepara a mensagem (Serializa para JSON e converte para Bytes)
        string json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        // 5. Publica!
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);
    }
}
