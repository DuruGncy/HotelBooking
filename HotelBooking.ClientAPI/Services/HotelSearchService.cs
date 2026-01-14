using HotelBooking.ClientAPI.Models;
using HotelBooking.ClientAPI.Models.DTOs;
using HotelBooking.ClientAPI.Services.Interfaces;
using HotelBooking.ClientAPI.Data;
using Microsoft.EntityFrameworkCore;
using RoomAvailabilityEntity = HotelBooking.ClientAPI.Data.Entities.RoomAvailability;

namespace HotelBooking.ClientAPI.Services;

public class HotelSearchService : IHotelSearchService
{
    private readonly ClientDbContext _context;
    private readonly ILogger<HotelSearchService> _logger;

    public HotelSearchService(ClientDbContext context, ILogger<HotelSearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<HotelSearchResponse>> SearchHotelsAsync(HotelSearchRequest request)
    {
        // Validate dates
        ValidateSearchRequest(request);

        try
        {
            // Search for hotels by destination (case-insensitive)
            var matchingHotels = await _context.Hotels
                .Where(h => h.Location.ToLower().Contains(request.Destination.ToLower()))
                .ToListAsync();

            if (!matchingHotels.Any())
            {
                return Enumerable.Empty<HotelSearchResponse>();
            }

            var results = new List<HotelSearchResponse>();
            var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;

            foreach (var hotel in matchingHotels)
            {
                var hotelRooms = await _context.Rooms
                    .Where(r => r.HotelId == hotel.Id)
                    .ToListAsync();
                
                var availableRooms = new List<AvailableRoomInfo>();

                foreach (var room in hotelRooms)
                {
                    // Check if room can accommodate the number of guests
                    if (room.MaxOccupancy < request.NumberOfGuests)
                        continue;

                    // Find available room count for the requested date range
                    var availability = await GetRoomAvailabilityAsync(room.Id, hotel.Id, request.CheckInDate, request.CheckOutDate);

                    if (availability != null && availability.AvailableRooms >= request.NumberOfRooms)
                    {
                        var totalPrice = availability.PricePerNight * numberOfNights * request.NumberOfRooms;
                        
                        availableRooms.Add(new AvailableRoomInfo
                        {
                            RoomId = room.Id,
                            RoomType = room.RoomType,
                            Description = room.Description,
                            MaxOccupancy = room.MaxOccupancy,
                            Amenities = room.Amenities,
                            PricePerNight = availability.PricePerNight,
                            AvailableCount = availability.AvailableRooms,
                            TotalPrice = totalPrice
                        });
                    }
                }

                // Only include hotels with available rooms
                if (availableRooms.Any())
                {
                    var lowestPrice = availableRooms.Min(r => r.PricePerNight);
                    var totalAvailableRooms = availableRooms.Sum(r => r.AvailableCount);

                    results.Add(new HotelSearchResponse
                    {
                        HotelId = hotel.Id,
                        HotelName = hotel.Name,
                        Location = hotel.Location,
                        Description = hotel.Description,
                        StarRating = hotel.StarRating,
                        AvailableRooms = availableRooms,
                        LowestPricePerNight = lowestPrice,
                        TotalPrice = lowestPrice * numberOfNights * request.NumberOfRooms,
                        TotalAvailableRooms = totalAvailableRooms
                    });
                }
            }

            // Sort by lowest price
            return results.OrderBy(r => r.LowestPricePerNight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching hotels with destination {Destination}", request.Destination);
            throw;
        }
    }

    public async Task<HotelSearchResponse?> GetHotelDetailsAsync(int hotelId, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
    {
        try
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId);
            if (hotel == null)
                return null;

            // Validate dates
            if (checkInDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Check-in date cannot be in the past");

            if (checkOutDate <= checkInDate)
                throw new ArgumentException("Check-out date must be after check-in date");

            var numberOfNights = (checkOutDate - checkInDate).Days;
            var hotelRooms = await _context.Rooms
                .Where(r => r.HotelId == hotel.Id)
                .ToListAsync();
            
            var availableRooms = new List<AvailableRoomInfo>();

            foreach (var room in hotelRooms)
            {
                var availability = await GetRoomAvailabilityAsync(room.Id, hotel.Id, checkInDate, checkOutDate);

                if (availability != null && availability.AvailableRooms > 0)
                {
                    var totalPrice = availability.PricePerNight * numberOfNights;

                    availableRooms.Add(new AvailableRoomInfo
                    {
                        RoomId = room.Id,
                        RoomType = room.RoomType,
                        Description = room.Description,
                        MaxOccupancy = room.MaxOccupancy,
                        Amenities = room.Amenities,
                        PricePerNight = availability.PricePerNight,
                        AvailableCount = availability.AvailableRooms,
                        TotalPrice = totalPrice
                    });
                }
            }

            if (!availableRooms.Any())
                return null;

            var lowestPrice = availableRooms.Min(r => r.PricePerNight);

            return new HotelSearchResponse
            {
                HotelId = hotel.Id,
                HotelName = hotel.Name,
                Location = hotel.Location,
                Description = hotel.Description,
                StarRating = hotel.StarRating,
                AvailableRooms = availableRooms,
                LowestPricePerNight = lowestPrice,
                TotalPrice = lowestPrice * numberOfNights,
                TotalAvailableRooms = availableRooms.Sum(r => r.AvailableCount)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hotel details for hotel {HotelId}", hotelId);
            throw;
        }
    }

    private async Task<RoomAvailabilityEntity?> GetRoomAvailabilityAsync(int roomId, int hotelId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            // Find availability records that cover the entire requested period
            var availabilityRecords = await _context.RoomAvailabilities
                .Where(a => 
                    a.RoomId == roomId && 
                    a.HotelId == hotelId &&
                    a.StartDate <= checkIn &&
                    a.EndDate >= checkOut &&
                    a.AvailableRooms > 0)
                .OrderByDescending(a => a.AvailableRooms)
                .FirstOrDefaultAsync();

            return availabilityRecords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room availability for room {RoomId} at hotel {HotelId}", roomId, hotelId);
            return null;
        }
    }

    private void ValidateSearchRequest(HotelSearchRequest request)
    {
        if (request.CheckInDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Check-in date cannot be in the past");
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date");
        }

        if (request.NumberOfGuests < 1)
        {
            throw new ArgumentException("Number of guests must be at least 1");
        }

        if (request.NumberOfRooms < 1)
        {
            throw new ArgumentException("Number of rooms must be at least 1");
        }
    }
}
