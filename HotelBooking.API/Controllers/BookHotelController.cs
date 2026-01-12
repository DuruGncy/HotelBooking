using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;
using System.Security.Claims;

namespace HotelBookingAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookHotelController : ControllerBase
{
    private readonly IBookHotelService _bookHotelService;

    public BookHotelController(IBookHotelService bookHotelService)
    {
        _bookHotelService = bookHotelService;
    }

    /// <summary>
    /// Create a new hotel booking
    /// </summary>
    /// <param name="request">Booking details</param>
    /// <returns>Created booking with booking reference</returns>
    [HttpPost("book")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        try
        {
            // Get user ID from JWT token if authenticated (optional)
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var booking = await _bookHotelService.CreateBookingAsync(request, userId);
            
            return CreatedAtAction(
                nameof(GetBookingById), 
                new { id = booking.BookingId }, 
                booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get booking details by ID
    /// </summary>
    /// <param name="id">Booking ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetBookingById(int id)
    {
        var booking = await _bookHotelService.GetBookingByIdAsync(id);
        
        if (booking == null)
        {
            return NotFound(new { error = $"Booking with ID {id} not found" });
        }

        return Ok(booking);
    }

    /// <summary>
    /// Get booking details by booking reference
    /// </summary>
    /// <param name="reference">Booking reference (e.g., BK20240315ABC123)</param>
    [HttpGet("reference/{reference}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetBookingByReference(string reference)
    {
        var booking = await _bookHotelService.GetBookingByReferenceAsync(reference);
        
        if (booking == null)
        {
            return NotFound(new { error = $"Booking with reference {reference} not found" });
        }

        return Ok(booking);
    }

    /// <summary>
    /// Get all bookings for the authenticated user
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user" });
        }

        var bookings = await _bookHotelService.GetUserBookingsAsync(userId);
        return Ok(bookings);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <param name="request">Cancellation details</param>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> CancelBooking(int id, [FromBody] CancelBookingRequest? request = null)
    {
        try
        {
            var booking = await _bookHotelService.CancelBookingAsync(id, request?.CancellationReason);
            return Ok(booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check availability for a specific room (before booking)
    /// </summary>
    [HttpGet("check-availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> CheckAvailability(
        [FromQuery] int hotelId,
        [FromQuery] int roomId,
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate,
        [FromQuery] int numberOfRooms = 1)
    {
        try
        {
            var isAvailable = await _bookHotelService.CheckAvailabilityAsync(
                hotelId, 
                roomId, 
                checkInDate, 
                checkOutDate, 
                numberOfRooms);

            return Ok(new
            {
                available = isAvailable,
                hotelId,
                roomId,
                checkInDate,
                checkOutDate,
                numberOfRooms,
                message = isAvailable 
                    ? "Room is available for the selected dates" 
                    : "Room is not available for the selected dates"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all bookings for a specific hotel (Admin only)
    /// </summary>
    [HttpGet("hotel/{hotelId}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetHotelBookings(
        int hotelId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var bookings = await _bookHotelService.GetHotelBookingsAsync(hotelId, startDate, endDate);
        return Ok(bookings);
    }
}
