using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TicketFlow.Api.BackgroundServices;

public class EmailWorker : BackgroundService
{
    // Configuração hardcoded para facilitar (em prod iria no appsettings)
    private readonly ConnectionFactory _factory;
    private readonly ILogger<EmailWorker> _logger;

    public EmailWorker(ILogger<EmailWorker> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory { HostName = "localhost" };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Conecta e cria o canal (igual ao Producer)
        // O Worker mantém a conexão aberta para sempre
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        // 2. Garante que a fila existe (Boa prática declarar nos dois lados)
        await channel.QueueDeclareAsync(
            queue: "ticket-sold-queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // 3. Configura o Consumidor
        var consumer = new AsyncEventingBasicConsumer(channel);

        // O que acontece quando chega mensagem?
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try 
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                
                // Simula o processamento
                _logger.LogInformation($"📧 [NOVA MENSAGEM] Processando envio de e-mail...");
                _logger.LogInformation($"📥 Conteúdo: {json}");

                // Simula delay de envio (ex: conectar no servidor SMTP)
                await Task.Delay(2000); 

                _logger.LogInformation("✅ E-mail enviado com sucesso!");

                // 4. ACK (Acknowledgement) - Avisa o RabbitMQ: "Pode apagar a mensagem, terminei."
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Erro ao processar mensagem: {ex.Message}");
                // Se der erro, poderíamos usar BasicNackAsync para devolver a mensagem pra fila
            }
        };

        // 5. Começa a ouvir a fila
        await channel.BasicConsumeAsync("ticket-sold-queue", autoAck: false, consumer);

        // Mantém o serviço rodando até alguém mandar parar
        _logger.LogInformation("👂 EmailWorker aguardando mensagens...");
        
        // Loop infinito seguro para manter a thread viva
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}