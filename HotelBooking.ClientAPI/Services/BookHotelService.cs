using HotelBooking.ClientAPI.Models;
using HotelBooking.ClientAPI.Models.DTOs;
using HotelBooking.ClientAPI.Services.Interfaces;
using HotelBooking.ClientAPI.Data;
using Microsoft.EntityFrameworkCore;
using BookingEntity = HotelBooking.ClientAPI.Data.Entities.Booking;
using RoomAvailabilityEntity = HotelBooking.ClientAPI.Data.Entities.RoomAvailability;

namespace HotelBooking.ClientAPI.Services;

public class BookHotelService : IBookHotelService
{
    private readonly INotificationClient _notificationClient;
    private readonly ILogger<BookHotelService> _logger;
    private readonly ClientDbContext _context;

    public BookHotelService(INotificationClient notificationClient, ILogger<BookHotelService> logger, ClientDbContext context)
    {
        _notificationClient = notificationClient;
        _logger = logger;
        _context = context;
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, int? userId = null)
    {
        try
        {
            // Validate dates
            ValidateBookingDates(request.CheckInDate, request.CheckOutDate);

            // Get hotel and room
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == request.HotelId);
            if (hotel == null)
                throw new KeyNotFoundException($"Hotel with ID {request.HotelId} not found");

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId && r.HotelId == request.HotelId);
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {request.RoomId} not found for hotel {request.HotelId}");

            // Check room capacity
            if (room.MaxOccupancy < request.NumberOfGuests)
                throw new InvalidOperationException($"Room capacity ({room.MaxOccupancy}) is less than number of guests ({request.NumberOfGuests})");

            // Check availability
            var availability = await GetRoomAvailabilityAsync(request.HotelId, request.RoomId, request.CheckInDate, request.CheckOutDate);
            if (availability == null)
                throw new InvalidOperationException("No availability found for the selected dates");

            if (availability.AvailableRooms < request.NumberOfRooms)
                throw new InvalidOperationException($"Only {availability.AvailableRooms} room(s) available, but {request.NumberOfRooms} requested");

            // Calculate pricing
            var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;
            var totalPrice = availability.PricePerNight * numberOfNights * request.NumberOfRooms;

            // Create booking reference
            var bookingReference = GenerateBookingReference();

            // Create booking entity
            var booking = new BookingEntity
            {
                BookingReference = bookingReference,
                UserId = userId ?? 0, // 0 for guest bookings
                HotelId = request.HotelId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfRooms = request.NumberOfRooms,
                NumberOfGuests = request.NumberOfGuests,
                PricePerNight = availability.PricePerNight,
                TotalPrice = totalPrice,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                GuestPhone = request.GuestPhone,
                SpecialRequests = request.SpecialRequests
            };

            // Add to database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(booking).Reference(b => b.Hotel).LoadAsync();
            await _context.Entry(booking).Reference(b => b.Room).LoadAsync();

            // DECREASE ROOM CAPACITY for the booking period
            availability.AvailableRooms -= request.NumberOfRooms;
            await _context.SaveChangesAsync();

            // Send booking confirmation notification to NotificationAPI
            try
            {
                _logger.LogInformation("Sending booking confirmation notification for booking {BookingId}", booking.Id);
                
                // Create model for notification client
                var bookingModel = new Models.Booking
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    RoomId = booking.RoomId,
                    GuestName = booking.GuestName,
                    GuestEmail = booking.GuestEmail,
                    BookingReference = booking.BookingReference,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfRooms = booking.NumberOfRooms,
                    NumberOfGuests = booking.NumberOfGuests,
                    TotalPrice = booking.TotalPrice,
                    SpecialRequests = booking.SpecialRequests
                };
                
                var notificationSent = await _notificationClient.SendBookingConfirmationAsync(bookingModel);
                
                if (notificationSent)
                {
                    _logger.LogInformation("Booking confirmation notification sent successfully for booking {BookingId}", booking.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send booking confirmation notification for booking {BookingId}", booking.Id);
                }
            }
            catch (Exception ex)
            {
                // Don't fail the booking if notification fails
                _logger.LogError(ex, "Error sending booking confirmation notification for booking {BookingId}", booking.Id);
            }

            return MapToResponse(booking, numberOfNights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            throw;
        }
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            
            if (booking == null)
                return null;

            var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
            return MapToResponse(booking, numberOfNights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by ID {BookingId}", bookingId);
            return null;
        }
    }

    public async Task<BookingResponse?> GetBookingByReferenceAsync(string bookingReference)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingReference.Equals(bookingReference, StringComparison.OrdinalIgnoreCase));
            
            if (booking == null)
                return null;

            var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
            return MapToResponse(booking, numberOfNights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by reference {BookingReference}", bookingReference);
            return null;
        }
    }

    public async Task<IEnumerable<BookingResponse>> GetUserBookingsAsync(int userId)
    {
        try
        {
            var bookings = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings.Select(b => MapToResponse(b, (b.CheckOutDate - b.CheckInDate).Days));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
            return Enumerable.Empty<BookingResponse>();
        }
    }

    public async Task<BookingResponse> CancelBookingAsync(int bookingId, string? cancellationReason = null)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {bookingId} not found");

            if (booking.Status == "Cancelled")
                throw new InvalidOperationException("Booking is already cancelled");

            if (booking.Status == "CheckedOut")
                throw new InvalidOperationException("Cannot cancel a completed booking");

            // Check if cancellation is allowed (e.g., at least 24 hours before check-in)
            if (booking.CheckInDate <= DateTime.UtcNow.AddHours(24))
                throw new InvalidOperationException("Cancellation must be at least 24 hours before check-in");

            // Update booking status
            booking.Status = "Cancelled";
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = cancellationReason;

            // RESTORE ROOM CAPACITY
            var availability = await GetRoomAvailabilityAsync(booking.HotelId, booking.RoomId, booking.CheckInDate, booking.CheckOutDate);
            if (availability != null)
            {
                availability.AvailableRooms += booking.NumberOfRooms;
            }

            await _context.SaveChangesAsync();

            // Send booking cancellation notification to NotificationAPI
            try
            {
                _logger.LogInformation("Sending booking cancellation notification for booking {BookingId}", booking.Id);
                
                // Create model for notification client
                var bookingModel = new Models.Booking
                {
                    Id = booking.Id,
                    HotelId = booking.HotelId,
                    GuestName = booking.GuestName,
                    GuestEmail = booking.GuestEmail,
                    BookingReference = booking.BookingReference,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    CancellationDate = booking.CancellationDate,
                    CancellationReason = booking.CancellationReason
                };
                
                var notificationSent = await _notificationClient.SendBookingCancellationAsync(bookingModel);
                
                if (notificationSent)
                {
                    _logger.LogInformation("Booking cancellation notification sent successfully for booking {BookingId}", booking.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send booking cancellation notification for booking {BookingId}", booking.Id);
                }
            }
            catch (Exception ex)
            {
                // Don't fail the cancellation if notification fails
                _logger.LogError(ex, "Error sending booking cancellation notification for booking {BookingId}", booking.Id);
            }

            var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
            return MapToResponse(booking, numberOfNights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<IEnumerable<BookingResponse>> GetHotelBookingsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .Where(b => b.HotelId == hotelId);

            if (startDate.HasValue)
                query = query.Where(b => b.CheckInDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= endDate.Value);

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings.Select(b => MapToResponse(b, (b.CheckOutDate - b.CheckInDate).Days));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for hotel {HotelId}", hotelId);
            return Enumerable.Empty<BookingResponse>();
        }
    }

    public async Task<bool> CheckAvailabilityAsync(int hotelId, int roomId, DateTime checkIn, DateTime checkOut, int numberOfRooms)
    {
        try
        {
            ValidateBookingDates(checkIn, checkOut);

            var availability = await GetRoomAvailabilityAsync(hotelId, roomId, checkIn, checkOut);
            
            return availability != null && availability.AvailableRooms >= numberOfRooms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for room {RoomId} at hotel {HotelId}", roomId, hotelId);
            return false;
        }
    }

    private async Task<RoomAvailabilityEntity?> GetRoomAvailabilityAsync(int hotelId, int roomId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            // Find availability that covers the entire booking period
            return await _context.RoomAvailabilities
                .Where(a =>
                    a.HotelId == hotelId &&
                    a.RoomId == roomId &&
                    a.StartDate <= checkIn &&
                    a.EndDate >= checkOut)
                .OrderByDescending(a => a.AvailableRooms)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room availability");
            return null;
        }
    }

    private void ValidateBookingDates(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn < DateTime.UtcNow.Date)
            throw new ArgumentException("Check-in date cannot be in the past");

        if (checkOut <= checkIn)
            throw new ArgumentException("Check-out date must be after check-in date");

        if ((checkOut - checkIn).Days > 30)
            throw new ArgumentException("Maximum booking duration is 30 nights");
    }

    private string GenerateBookingReference()
    {
        // Generate a unique booking reference (e.g., BK20240315ABC123)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"BK{timestamp}{random}";
    }

    private BookingResponse MapToResponse(BookingEntity booking, int numberOfNights)
    {
        return new BookingResponse
        {
            BookingId = booking.Id,
            BookingReference = booking.BookingReference,
            HotelId = booking.HotelId,
            HotelName = booking.Hotel?.Name ?? string.Empty,
            HotelLocation = booking.Hotel?.Location ?? string.Empty,
            RoomId = booking.RoomId,
            RoomType = booking.Room?.RoomType ?? string.Empty,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfNights = numberOfNights,
            NumberOfRooms = booking.NumberOfRooms,
            NumberOfGuests = booking.NumberOfGuests,
            PricePerNight = booking.PricePerNight,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingDate = booking.BookingDate,
            GuestName = booking.GuestName,
            GuestEmail = booking.GuestEmail,
            GuestPhone = booking.GuestPhone,
            SpecialRequests = booking.SpecialRequests
        };
    }
}
