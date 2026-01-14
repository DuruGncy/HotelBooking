using HotelBooking.AdminAPI.Models.DTOs;
using HotelBooking.AdminAPI.Services.Interfaces;
using HotelBooking.AdminAPI.Data;
using HotelBooking.AdminAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.AdminAPI.Services;

public class AdminHotelService : IAdminHotelService
{
    private readonly AdminDbContext _context;

    public AdminHotelService(AdminDbContext context)
    {
        _context = context;
    }

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

        // Validate hotel and room exist
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == request.HotelId);
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId && r.HotelId == request.HotelId);

        if (hotel == null)
        {
            throw new KeyNotFoundException($"Hotel with ID {request.HotelId} not found");
        }

        if (room == null)
        {
            throw new KeyNotFoundException($"Room with ID {request.RoomId} not found for hotel {request.HotelId}");
        }

        // Check for overlapping availability periods
        var hasOverlap = await _context.RoomAvailabilities.AnyAsync(a =>
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
            RoomId = request.RoomId,
            HotelId = request.HotelId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AvailableRooms = request.AvailableRooms,
            PricePerNight = request.PricePerNight,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomAvailabilities.Add(availability);
        await _context.SaveChangesAsync();

        // Load navigation properties
        await _context.Entry(availability)
            .Reference(a => a.Hotel)
            .LoadAsync();
        await _context.Entry(availability)
            .Reference(a => a.Room)
            .LoadAsync();

        return MapToResponse(availability);
    }

    public async Task<RoomAvailabilityResponse> UpdateRoomAvailabilityAsync(UpdateRoomAvailabilityRequest request)
    {
        var availability = await _context.RoomAvailabilities
            .Include(a => a.Hotel)
            .Include(a => a.Room)
            .FirstOrDefaultAsync(a => a.Id == request.AvailabilityId);
        
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
            var hasOverlap = await _context.RoomAvailabilities.AnyAsync(a =>
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
        
        await _context.SaveChangesAsync();

        return MapToResponse(availability);
    }

    public async Task<bool> DeleteRoomAvailabilityAsync(int availabilityId)
    {
        var availability = await _context.RoomAvailabilities.FindAsync(availabilityId);
        
        if (availability == null)
        {
            return false;
        }

        _context.RoomAvailabilities.Remove(availability);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<RoomAvailabilityResponse?> GetRoomAvailabilityByIdAsync(int availabilityId)
    {
        var availability = await _context.RoomAvailabilities
            .Include(a => a.Hotel)
            .Include(a => a.Room)
            .FirstOrDefaultAsync(a => a.Id == availabilityId);
            
        return availability != null ? MapToResponse(availability) : null;
    }

    public async Task<IEnumerable<RoomAvailabilityResponse>> GetRoomAvailabilitiesByHotelAsync(int hotelId)
    {
        var availabilities = await _context.RoomAvailabilities
            .Include(a => a.Hotel)
            .Include(a => a.Room)
            .Where(a => a.HotelId == hotelId)
            .OrderBy(a => a.StartDate)
            .ToListAsync();

        return availabilities.Select(MapToResponse);
    }

    public async Task<IEnumerable<Data.Entities.Hotel>> GetAllHotelsAsync()
    {
        return await _context.Hotels
            .Where(h => h.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Data.Entities.Room>> GetAllRoomsAsync()
    {
        return await _context.Rooms
            .Include(r => r.Hotel)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Data.Entities.RoomAvailability>> GetAllAvailabilitiesAsync()
    {
        return await _context.RoomAvailabilities
            .Include(ra => ra.Hotel)
            .Include(ra => ra.Room)
            .ToListAsync();
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
}
