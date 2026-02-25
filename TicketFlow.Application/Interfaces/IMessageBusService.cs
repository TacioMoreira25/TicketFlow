using System;

namespace TicketFlow.Application.Interfaces;

public interface IMessageBusService
{
    Task PublishAsync<T>(string queue, T message);
}
