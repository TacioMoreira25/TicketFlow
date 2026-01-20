using System;
using TicketFlow.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TicketFlow.Infrastructure.Services;

public class RabbitMqService : IMessageBusService
{
    // Configurações básicas para rodar local (Docker)
    private readonly ConnectionFactory _factory;

    public RabbitMqService()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost"
            // Se tivesse usuário/senha diferentes de 'guest', colocaria aqui
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
