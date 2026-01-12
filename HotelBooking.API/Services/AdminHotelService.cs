using HotelBookingAPI.Models;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Services;

public class AdminHotelService : IAdminHotelService
{
    // In a real application, replace this with a database context (e.g., DbContext)
    private static readonly List<RoomAvailability> _availabilities = new();
    private static readonly List<Hotel> _hotels = new();
    private static readonly List<Room> _rooms = new();
    private static int _nextId = 1;

    public async Task<RoomAvailabilityResponse> AddRoomAvailabilityAsync(AddRoomAvailabilityRequest request)
    {
        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        if (request.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Start date cannot be in the past");
        }

        // Validate hotel and room exist (in real app, check database)
        var hotel = _hotels.FirstOrDefault(h => h.Id == request.HotelId);
        var room = _rooms.FirstOrDefault(r => r.Id == request.RoomId && r.HotelId == request.HotelId);

        if (hotel == null)
        {
            throw new KeyNotFoundException($"Hotel with ID {request.HotelId} not found");
        }

        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {request.RoomId} not found for hotel {request.HotelId}");
        }

        // Check for overlapping availability periods
        var hasOverlap = _availabilities.Any(a =>
            a.RoomId == request.RoomId &&
            a.HotelId == request.HotelId &&
            a.StartDate < request.EndDate &&
            a.EndDate > request.StartDate);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Room availability overlaps with existing availability period");
        }

        var availability = new RoomAvailability
        {
            Id = _nextId++,
            RoomId = request.RoomId,
            HotelId = request.HotelId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AvailableRooms = request.AvailableRooms,
            PricePerNight = request.PricePerNight,
            CreatedAt = DateTime.UtcNow,
            Hotel = hotel,
            Room = room
        };

        _availabilities.Add(availability);
        
        // Sync data with HotelSearchService
        SyncDataWithSearchService();

        return await Task.FromResult(MapToResponse(availability));
    }

    public async Task<RoomAvailabilityResponse> UpdateRoomAvailabilityAsync(UpdateRoomAvailabilityRequest request)
    {
        var availability = _availabilities.FirstOrDefault(a => a.Id == request.AvailabilityId);
        
        if (availability == null)
        {
            throw new KeyNotFoundException($"Room availability with ID {request.AvailabilityId} not found");
        }

        // Update only provided fields
        if (request.AvailableRooms.HasValue)
        {
            availability.AvailableRooms = request.AvailableRooms.Value;
        }

        if (request.PricePerNight.HasValue)
        {
            availability.PricePerNight = request.PricePerNight.Value;
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var newStartDate = request.StartDate ?? availability.StartDate;
            var newEndDate = request.EndDate ?? availability.EndDate;

            if (newStartDate >= newEndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Check for overlapping availability periods (excluding current record)
            var hasOverlap = _availabilities.Any(a =>
                a.Id != availability.Id &&
                a.RoomId == availability.RoomId &&
                a.HotelId == availability.HotelId &&
                a.StartDate < newEndDate &&
                a.EndDate > newStartDate);

            if (hasOverlap)
            {
                throw new InvalidOperationException("Updated dates would overlap with existing availability period");
            }

            availability.StartDate = newStartDate;
            availability.EndDate = newEndDate;
        }

        availability.UpdatedAt = DateTime.UtcNow;
        
        // Sync data with HotelSearchService
        SyncDataWithSearchService();

        return await Task.FromResult(MapToResponse(availability));
    }

    public async Task<bool> DeleteRoomAvailabilityAsync(int availabilityId)
    {
        var availability = _availabilities.FirstOrDefault(a => a.Id == availabilityId);
        
        if (availability == null)
        {
            return await Task.FromResult(false);
        }

        _availabilities.Remove(availability);
        
        // Sync data with HotelSearchService
        SyncDataWithSearchService();
        
        return await Task.FromResult(true);
    }

    public async Task<RoomAvailabilityResponse?> GetRoomAvailabilityByIdAsync(int availabilityId)
    {
        var availability = _availabilities.FirstOrDefault(a => a.Id == availabilityId);
        return await Task.FromResult(availability != null ? MapToResponse(availability) : null);
    }

    public async Task<IEnumerable<RoomAvailabilityResponse>> GetRoomAvailabilitiesByHotelAsync(int hotelId)
    {
        var availabilities = _availabilities
            .Where(a => a.HotelId == hotelId)
            .OrderBy(a => a.StartDate)
            .Select(MapToResponse);

        return await Task.FromResult(availabilities);
    }

    private static RoomAvailabilityResponse MapToResponse(RoomAvailability availability)
    {
        return new RoomAvailabilityResponse
        {
            Id = availability.Id,
            HotelId = availability.HotelId,
            HotelName = availability.Hotel?.Name,
            RoomId = availability.RoomId,
            RoomType = availability.Room?.RoomType,
            StartDate = availability.StartDate,
            EndDate = availability.EndDate,
            AvailableRooms = availability.AvailableRooms,
            PricePerNight = availability.PricePerNight,
            CreatedAt = availability.CreatedAt,
            UpdatedAt = availability.UpdatedAt
        };
    }

    private static void SyncDataWithSearchService()
    {
        // Share data with HotelSearchService
        HotelSearchService.InitializeData(_hotels, _rooms, _availabilities);
    }

    // Helper method to seed data for testing (call this from Program.cs if needed)
    public static void SeedTestData()
    {
        _hotels.Clear();
        _rooms.Clear();
        _availabilities.Clear();
        
        // Seed hotels with detailed information
        _hotels.Add(new Hotel 
        { 
            Id = 1, 
            Name = "Grand Plaza Hotel", 
            Location = "New York", 
            Description = "Luxury hotel in the heart of Manhattan with stunning city views",
            StarRating = 5 
        });
        
        _hotels.Add(new Hotel 
        { 
            Id = 2, 
            Name = "Seaside Resort", 
            Location = "Miami", 
            Description = "Beachfront resort with private beach access and spa facilities",
            StarRating = 4 
        });

        // Seed rooms with amenities
        _rooms.Add(new Room 
        { 
            Id = 1, 
            HotelId = 1, 
            RoomType = "Deluxe Suite", 
            MaxOccupancy = 2,
            Description = "Spacious suite with king bed and city view",
            Amenities = new List<string> { "WiFi", "TV", "Mini Bar", "Coffee Maker", "Safe" }
        });
        
        _rooms.Add(new Room 
        { 
            Id = 2, 
            HotelId = 1, 
            RoomType = "Standard Room", 
            MaxOccupancy = 2,
            Description = "Comfortable room with queen bed",
            Amenities = new List<string> { "WiFi", "TV", "Coffee Maker" }
        });
        
        _rooms.Add(new Room 
        { 
            Id = 3, 
            HotelId = 2, 
            RoomType = "Ocean View Suite", 
            MaxOccupancy = 4,
            Description = "Large suite with ocean view and balcony",
            Amenities = new List<string> { "WiFi", "TV", "Mini Bar", "Balcony", "Ocean View", "Jacuzzi" }
        });

        // Seed some initial availabilities for testing
        var today = DateTime.UtcNow.Date;
        
        _availabilities.Add(new RoomAvailability
        {
            Id = _nextId++,
            RoomId = 1,
            HotelId = 1,
            StartDate = today.AddDays(1),
            EndDate = today.AddDays(90),
            AvailableRooms = 5,
            PricePerNight = 250.00m,
            CreatedAt = DateTime.UtcNow,
            Hotel = _hotels[0],
            Room = _rooms[0]
        });

        _availabilities.Add(new RoomAvailability
        {
            Id = _nextId++,
            RoomId = 2,
            HotelId = 1,
            StartDate = today.AddDays(1),
            EndDate = today.AddDays(90),
            AvailableRooms = 10,
            PricePerNight = 150.00m,
            CreatedAt = DateTime.UtcNow,
            Hotel = _hotels[0],
            Room = _rooms[1]
        });

        _availabilities.Add(new RoomAvailability
        {
            Id = _nextId++,
            RoomId = 3,
            HotelId = 2,
            StartDate = today.AddDays(1),
            EndDate = today.AddDays(90),
            AvailableRooms = 8,
            PricePerNight = 300.00m,
            CreatedAt = DateTime.UtcNow,
            Hotel = _hotels[1],
            Room = _rooms[2]
        });
        
        // Sync with search service
        SyncDataWithSearchService();
    }
}
