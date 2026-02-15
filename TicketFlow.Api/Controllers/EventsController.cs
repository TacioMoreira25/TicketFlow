using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.DTOs;
using TicketFlow.Application.Interfaces;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;
    private readonly IValidator<CreateEventRequest> _validator;

    // Injetamos a Interface do Serviço e o Validador
    public EventsController(IEventService service, IValidator<CreateEventRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var result = await _service.CreateAsync(request);
        // Retorna 201 Created
        return CreatedAtAction(nameof(GetAll), null, result);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyTicketRequest request)
    {
        try
        {
            var sucesso = await _service.BuyTicketAsync(request.TicketId, request.OwnerName);
            if (!sucesso) return NotFound("Ingresso não encontrado.");

            return Ok("Compra realizada com sucesso!");
        }
        catch (Exception ex)
        {
            // Retorna 409 Conflict - O código HTTP correto para "Tentei mas deu conflito"
            return Conflict(new { message = ex.Message });
        }
    }

    // POST: api/events/{id}/tickets
    [HttpPost("{id}/tickets")]
    public async Task<IActionResult> CreateTickets(Guid id, [FromBody] CreateTicketsRequest request)
    {
        await _service.CreateTicketsAsync(id, request.Quantity, request.Price);
        return Ok($"{request.Quantity} ingressos criados para o evento {id}!");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _service.GetAllAsync();
        return Ok(events);
    }

    // GET: api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var evento = await _service.GetByIdAsync(id);

        if (evento == null)
        {
            return NotFound();
        }

        return Ok(evento);
    }
}
