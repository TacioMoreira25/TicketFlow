using System;

namespace TicketFlow.Application.Interfaces;

public interface IMessageBusService
{
    // Método genérico: Aceita qualquer tipo de objeto (T) e manda para uma fila
    Task PublishAsync<T>(string queue, T message);
}
