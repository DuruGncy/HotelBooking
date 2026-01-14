using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.AdminAPI.Models.DTOs;
using HotelBooking.AdminAPI.Services.Interfaces;

namespace HotelBooking.AdminAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminHotelService _adminHotelService;

    public AdminController(IAdminHotelService adminHotelService)
    {
        _adminHotelService = adminHotelService;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "Admin API",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            description = "Admin API for managing hotel rooms and availability",
            database = "Aurora PostgreSQL"
        });
    }

    /// <summary>
    /// Add room availability for a specific date range
    /// </summary>
    [HttpPost("room-availability")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomAvailabilityResponse>> AddRoomAvailability([FromBody] AddRoomAvailabilityRequest request)
    {
        try
        {
            var result = await _adminHotelService.AddRoomAvailabilityAsync(request);
            return CreatedAtAction(nameof(GetRoomAvailability), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
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
    /// Update existing room availability
    /// </summary>
    [HttpPut("room-availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomAvailabilityResponse>> UpdateRoomAvailability([FromBody] UpdateRoomAvailabilityRequest request)
    {
        try
        {
            var result = await _adminHotelService.UpdateRoomAvailabilityAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
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
    /// Delete room availability by ID
    /// </summary>
    [HttpDelete("room-availability/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoomAvailability(int id)
    {
        var result = await _adminHotelService.DeleteRoomAvailabilityAsync(id);

        if (!result)
        {
            return NotFound(new { error = $"Room availability with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Get room availability by ID
    /// </summary>
    [HttpGet("room-availability/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomAvailabilityResponse>> GetRoomAvailability(int id)
    {
        var result = await _adminHotelService.GetRoomAvailabilityByIdAsync(id);

        if (result == null)
        {
            return NotFound(new { error = $"Room availability with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all room availabilities for a specific hotel
    /// </summary>
    [HttpGet("hotels/{hotelId}/room-availabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<RoomAvailabilityResponse>>> GetRoomAvailabilitiesByHotel(int hotelId)
    {
        var result = await _adminHotelService.GetRoomAvailabilitiesByHotelAsync(hotelId);
        return Ok(result);
    }

    /// <summary>
    /// Get all hotels (for viewing)
    /// </summary>
    [HttpGet("hotels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHotels()
    {
        var hotels = await _adminHotelService.GetAllHotelsAsync();
        return Ok(hotels);
    }

    /// <summary>
    /// Get all rooms (for viewing)
    /// </summary>
    [HttpGet("rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _adminHotelService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    /// <summary>
    /// Get all room availabilities (for viewing)
    /// </summary>
    [HttpGet("availabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailabilities()
    {
        var availabilities = await _adminHotelService.GetAllAvailabilitiesAsync();
        return Ok(availabilities);
    }
}
