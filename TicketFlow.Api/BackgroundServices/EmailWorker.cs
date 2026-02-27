using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TicketFlow.Api.BackgroundServices;

public class EmailWorker : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<EmailWorker> _logger;

    public EmailWorker(ILogger<EmailWorker> logger, IConfiguration configuration)
    {
        _logger = logger;
        var rabbitConnectionString = configuration.GetConnectionString("rabbitmq-bus") ?? "";
        _factory = new ConnectionFactory 
        { 
            Uri = new Uri(rabbitConnectionString)
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ticket-sold-queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try 
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation($" [NOVA MENSAGEM] Processando envio de e-mail...");
                _logger.LogInformation($" Conteúdo: {json}");

                await Task.Delay(2000); 

                _logger.LogInformation("E-mail enviado com sucesso!");

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar mensagem: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync("ticket-sold-queue", autoAck: false, consumer);

        _logger.LogInformation("EmailWorker aguardando mensagens...");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}