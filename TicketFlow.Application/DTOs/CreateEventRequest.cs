using System.ComponentModel.DataAnnotations;

namespace TicketFlow.Application.DTOs;

public record CreateEventRequest(
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters long")]
    string Title, 
    
    [Required]
    DateTime Date, 
    
    [Required]
    [StringLength(500)]
    string Description);