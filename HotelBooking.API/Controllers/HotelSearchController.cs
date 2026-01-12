using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HotelSearchController : ControllerBase
{
    private readonly IHotelSearchService _hotelSearchService;

    public HotelSearchController(IHotelSearchService hotelSearchService)
    {
        _hotelSearchService = hotelSearchService;
    }

    /// <summary>
    /// Search for available hotels based on destination, dates, and number of guests
    /// </summary>
    /// <param name="request">Search criteria including destination, check-in/out dates, and number of guests</param>
    /// <returns>List of hotels with available rooms matching the search criteria</returns>
    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<HotelSearchResponse>>> SearchHotels([FromBody] HotelSearchRequest request)
    {
        try
        {
            var results = await _hotelSearchService.SearchHotelsAsync(request);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Search for hotels using query parameters (alternative to POST)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<HotelSearchResponse>>> SearchHotelsGet(
        [FromQuery] string destination,
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate,
        [FromQuery] int numberOfGuests,
        [FromQuery] int numberOfRooms = 1)
    {
        try
        {
            var request = new HotelSearchRequest
            {
                Destination = destination,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                NumberOfGuests = numberOfGuests,
                NumberOfRooms = numberOfRooms
            };

            // Validate the model manually since it's not coming from body
            if (string.IsNullOrWhiteSpace(destination))
            {
                return BadRequest(new { error = "Destination is required" });
            }

            var results = await _hotelSearchService.SearchHotelsAsync(request);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed availability information for a specific hotel
    /// </summary>
    /// <param name="hotelId">Hotel ID</param>
    /// <param name="checkInDate">Check-in date</param>
    /// <param name="checkOutDate">Check-out date</param>
    /// <param name="numberOfGuests">Number of guests</param>
    [HttpGet("hotels/{hotelId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelSearchResponse>> GetHotelDetails(
        int hotelId,
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate,
        [FromQuery] int numberOfGuests)
    {
        try
        {
            var result = await _hotelSearchService.GetHotelDetailsAsync(hotelId, checkInDate, checkOutDate, numberOfGuests);
            
            if (result == null)
            {
                return NotFound(new { error = $"Hotel with ID {hotelId} not found or no rooms available for the specified dates" });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Quick search endpoint with minimal parameters
    /// </summary>
    [HttpGet("quick-search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<HotelSearchResponse>>> QuickSearch(
        [FromQuery] string destination,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int guests = 2)
    {
        try
        {
            var request = new HotelSearchRequest
            {
                Destination = destination,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NumberOfGuests = guests,
                NumberOfRooms = 1
            };

            if (string.IsNullOrWhiteSpace(destination))
            {
                return BadRequest(new { error = "Destination is required" });
            }

            var results = await _hotelSearchService.SearchHotelsAsync(request);
            
            // Return simplified results for quick search
            var simplifiedResults = results.Select(r => new
            {
                r.HotelId,
                r.HotelName,
                r.Location,
                r.StarRating,
                r.LowestPricePerNight,
                r.TotalPrice,
                r.TotalAvailableRooms
            });

            return Ok(simplifiedResults);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
