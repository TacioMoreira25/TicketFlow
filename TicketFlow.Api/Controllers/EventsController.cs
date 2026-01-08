using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.DTOs;
using TicketFlow.Application.Interfaces;

namespace TicketFlow.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    // Injetamos a Interface do Serviço
    public EventsController(IEventService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var result = await _service.CreateAsync(request);
        // Retorna 201 Created
        return CreatedAtAction(nameof(GetAll), null, result);
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
