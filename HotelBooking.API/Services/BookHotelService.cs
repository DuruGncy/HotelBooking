using HotelBookingAPI.Models;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Services;

public class BookHotelService : IBookHotelService
{
    // In a real application, replace this with database context
    private static readonly List<Booking> _bookings = new();
    private static readonly List<Hotel> _hotels = new();
    private static readonly List<Room> _rooms = new();
    private static readonly List<RoomAvailability> _availabilities = new();
    private static readonly List<User> _users = new();
    private static int _nextBookingId = 1;

    // Initialize data from AdminHotelService
    public static void InitializeData(List<Hotel> hotels, List<Room> rooms, List<RoomAvailability> availabilities)
    {
        _hotels.Clear();
        _hotels.AddRange(hotels);
        _rooms.Clear();
        _rooms.AddRange(rooms);
        _availabilities.Clear();
        _availabilities.AddRange(availabilities);
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, int? userId = null)
    {
        // Validate dates
        ValidateBookingDates(request.CheckInDate, request.CheckOutDate);

        // Get hotel and room
        var hotel = _hotels.FirstOrDefault(h => h.Id == request.HotelId);
        if (hotel == null)
            throw new KeyNotFoundException($"Hotel with ID {request.HotelId} not found");

        var room = _rooms.FirstOrDefault(r => r.Id == request.RoomId && r.HotelId == request.HotelId);
        if (room == null)
            throw new KeyNotFoundException($"Room with ID {request.RoomId} not found for hotel {request.HotelId}");

        // Check room capacity
        if (room.MaxOccupancy < request.NumberOfGuests)
            throw new InvalidOperationException($"Room capacity ({room.MaxOccupancy}) is less than number of guests ({request.NumberOfGuests})");

        // Check availability
        var availability = GetRoomAvailability(request.HotelId, request.RoomId, request.CheckInDate, request.CheckOutDate);
        if (availability == null)
            throw new InvalidOperationException("No availability found for the selected dates");

        if (availability.AvailableRooms < request.NumberOfRooms)
            throw new InvalidOperationException($"Only {availability.AvailableRooms} room(s) available, but {request.NumberOfRooms} requested");

        // Calculate pricing
        var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;
        var totalPrice = availability.PricePerNight * numberOfNights * request.NumberOfRooms;

        // Create booking reference
        var bookingReference = GenerateBookingReference();

        // Create booking
        var booking = new Booking
        {
            Id = _nextBookingId++,
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
            Status = BookingStatus.Confirmed,
            BookingDate = DateTime.UtcNow,
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            SpecialRequests = request.SpecialRequests,
            Hotel = hotel,
            Room = room
        };

        _bookings.Add(booking);

        // DECREASE ROOM CAPACITY for the booking period
        availability.AvailableRooms -= request.NumberOfRooms;

        // Sync changes back to AdminHotelService
        AdminHotelService.SeedTestData(); // This will be replaced with database in production

        return await Task.FromResult(MapToResponse(booking, numberOfNights));
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(int bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
            return null;

        var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        return await Task.FromResult(MapToResponse(booking, numberOfNights));
    }

    public async Task<BookingResponse?> GetBookingByReferenceAsync(string bookingReference)
    {
        var booking = _bookings.FirstOrDefault(b => b.BookingReference.Equals(bookingReference, StringComparison.OrdinalIgnoreCase));
        if (booking == null)
            return null;

        var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        return await Task.FromResult(MapToResponse(booking, numberOfNights));
    }

    public async Task<IEnumerable<BookingResponse>> GetUserBookingsAsync(int userId)
    {
        var bookings = _bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => MapToResponse(b, (b.CheckOutDate - b.CheckInDate).Days));

        return await Task.FromResult(bookings);
    }

    public async Task<BookingResponse> CancelBookingAsync(int bookingId, string? cancellationReason = null)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
            throw new KeyNotFoundException($"Booking with ID {bookingId} not found");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled");

        if (booking.Status == BookingStatus.CheckedOut)
            throw new InvalidOperationException("Cannot cancel a completed booking");

        // Check if cancellation is allowed (e.g., at least 24 hours before check-in)
        if (booking.CheckInDate <= DateTime.UtcNow.AddHours(24))
            throw new InvalidOperationException("Cancellation must be at least 24 hours before check-in");

        // Update booking status
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = DateTime.UtcNow;
        booking.CancellationReason = cancellationReason;

        // RESTORE ROOM CAPACITY
        var availability = GetRoomAvailability(booking.HotelId, booking.RoomId, booking.CheckInDate, booking.CheckOutDate);
        if (availability != null)
        {
            availability.AvailableRooms += booking.NumberOfRooms;
        }

        var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        return await Task.FromResult(MapToResponse(booking, numberOfNights));
    }

    public async Task<IEnumerable<BookingResponse>> GetHotelBookingsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _bookings.Where(b => b.HotelId == hotelId);

        if (startDate.HasValue)
            query = query.Where(b => b.CheckInDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(b => b.CheckOutDate <= endDate.Value);

        var bookings = query
            .OrderByDescending(b => b.BookingDate)
            .Select(b => MapToResponse(b, (b.CheckOutDate - b.CheckInDate).Days));

        return await Task.FromResult(bookings);
    }

    public async Task<bool> CheckAvailabilityAsync(int hotelId, int roomId, DateTime checkIn, DateTime checkOut, int numberOfRooms)
    {
        ValidateBookingDates(checkIn, checkOut);

        var availability = GetRoomAvailability(hotelId, roomId, checkIn, checkOut);
        
        return await Task.FromResult(availability != null && availability.AvailableRooms >= numberOfRooms);
    }

    private RoomAvailability? GetRoomAvailability(int hotelId, int roomId, DateTime checkIn, DateTime checkOut)
    {
        // Find availability that covers the entire booking period
        return _availabilities
            .Where(a =>
                a.HotelId == hotelId &&
                a.RoomId == roomId &&
                a.StartDate <= checkIn &&
                a.EndDate >= checkOut)
            .OrderByDescending(a => a.AvailableRooms)
            .FirstOrDefault();
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

    private BookingResponse MapToResponse(Booking booking, int numberOfNights)
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
            Status = booking.Status.ToString(),
            BookingDate = booking.BookingDate,
            GuestName = booking.GuestName,
            GuestEmail = booking.GuestEmail,
            GuestPhone = booking.GuestPhone,
            SpecialRequests = booking.SpecialRequests
        };
    }

    // Helper method to get all bookings (for admin)
    public static List<Booking> GetAllBookings()
    {
        return _bookings;
    }
}
