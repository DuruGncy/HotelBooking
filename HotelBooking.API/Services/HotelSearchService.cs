using HotelBookingAPI.Models;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Services;

public class HotelSearchService : IHotelSearchService
{
    // In a real application, replace this with database access via DbContext
    // For now, we'll access the same in-memory data as AdminHotelService
    private static readonly List<Hotel> _hotels = new();
    private static readonly List<Room> _rooms = new();
    private static readonly List<RoomAvailability> _availabilities = new();

    // This method allows AdminHotelService to share data (temporary solution for demo)
    public static void InitializeData(List<Hotel> hotels, List<Room> rooms, List<RoomAvailability> availabilities)
    {
        _hotels.Clear();
        _hotels.AddRange(hotels);
        _rooms.Clear();
        _rooms.AddRange(rooms);
        _availabilities.Clear();
        _availabilities.AddRange(availabilities);
    }

    public async Task<IEnumerable<HotelSearchResponse>> SearchHotelsAsync(HotelSearchRequest request)
    {
        // Validate dates
        ValidateSearchRequest(request);

        // Search for hotels by destination (case-insensitive)
        var matchingHotels = _hotels
            .Where(h => h.Location.Contains(request.Destination, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!matchingHotels.Any())
        {
            return Enumerable.Empty<HotelSearchResponse>();
        }

        var results = new List<HotelSearchResponse>();
        var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;

        foreach (var hotel in matchingHotels)
        {
            var hotelRooms = _rooms.Where(r => r.HotelId == hotel.Id).ToList();
            var availableRooms = new List<AvailableRoomInfo>();

            foreach (var room in hotelRooms)
            {
                // Check if room can accommodate the number of guests
                if (room.MaxOccupancy < request.NumberOfGuests)
                    continue;

                // Find available room count for the requested date range
                var availability = GetRoomAvailability(room.Id, hotel.Id, request.CheckInDate, request.CheckOutDate);

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
        return await Task.FromResult(results.OrderBy(r => r.LowestPricePerNight));
    }

    public async Task<HotelSearchResponse?> GetHotelDetailsAsync(int hotelId, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
    {
        var hotel = _hotels.FirstOrDefault(h => h.Id == hotelId);
        if (hotel == null)
            return null;

        // Validate dates
        if (checkInDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Check-in date cannot be in the past");

        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out date must be after check-in date");

        var numberOfNights = (checkOutDate - checkInDate).Days;
        var hotelRooms = _rooms.Where(r => r.HotelId == hotel.Id).ToList();
        var availableRooms = new List<AvailableRoomInfo>();

        foreach (var room in hotelRooms)
        {
            var availability = GetRoomAvailability(room.Id, hotel.Id, checkInDate, checkOutDate);

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

        return await Task.FromResult(new HotelSearchResponse
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
        });
    }

    private RoomAvailability? GetRoomAvailability(int roomId, int hotelId, DateTime checkIn, DateTime checkOut)
    {
        // Find availability records that cover the entire requested period
        var availabilityRecords = _availabilities
            .Where(a => 
                a.RoomId == roomId && 
                a.HotelId == hotelId &&
                a.StartDate <= checkIn &&
                a.EndDate >= checkOut &&
                a.AvailableRooms > 0)
            .OrderByDescending(a => a.AvailableRooms)
            .FirstOrDefault();

        return availabilityRecords;
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
