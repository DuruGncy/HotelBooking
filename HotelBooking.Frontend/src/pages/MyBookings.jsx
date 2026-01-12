import React, { useState } from 'react';
import { getBookingByReference, cancelBooking } from '../services/api';
import LoadingSpinner from '../components/LoadingSpinner';

const MyBookings = () => {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [searchReference, setSearchReference] = useState('');
  const [searchLoading, setSearchLoading] = useState(false);

  const handleSearchByReference = async (e) => {
    e.preventDefault();
    if (!searchReference.trim()) return;

    setSearchLoading(true);
    setError(null);

    try {
      const booking = await getBookingByReference(searchReference);
      setBookings([booking]);
    } catch (err) {
      setError(err.message || 'Booking not found');
      setBookings([]);
    } finally {
      setSearchLoading(false);
    }
  };

  const handleCancelBooking = async (bookingId) => {
    if (!window.confirm('Are you sure you want to cancel this booking?')) {
      return;
    }

    try {
      const reason = prompt('Please enter cancellation reason:');
      await cancelBooking(bookingId, reason || 'User requested cancellation');
      
      setBookings((prev) =>
        prev.map((b) =>
          b.bookingId === bookingId ? { ...b, status: 'Cancelled' } : b
        )
      );

      alert('Booking cancelled successfully');
    } catch (err) {
      alert(err.message || 'Failed to cancel booking');
    }
  };

  const canCancel = (checkInDate) => {
    const checkIn = new Date(checkInDate);
    const now = new Date();
    const hoursDiff = (checkIn - now) / (1000 * 60 * 60);
    return hoursDiff >= 24;
  };

  return (
    <div>
      <h1>My Bookings</h1>

      {/* Search by Reference */}
      <div className="card mt-2">
        <h3>Find Booking by Reference</h3>
        <form onSubmit={handleSearchByReference} className="flex gap-2">
          <input
            type="text"
            value={searchReference}
            onChange={(e) => setSearchReference(e.target.value)}
            placeholder="Enter booking reference (e.g., BK20240315...)"
            style={{ flex: 1 }}
          />
          <button type="submit" className="btn btn-primary" disabled={searchLoading}>
            {searchLoading ? 'Searching...' : 'Search'}
          </button>
        </form>
      </div>

      {error && (
        <div className="alert alert-error mt-2">
          <strong>Error:</strong> {error}
        </div>
      )}

      {/* Bookings List */}
      <div className="mt-3">
        {bookings.length === 0 && !error && (
          <div className="card text-center">
            <p>No bookings found. Search by booking reference above.</p>
          </div>
        )}
        
        {bookings.map((booking) => (
          <div key={booking.bookingId} className="card mb-2">
            <div className="flex-between">
              <div style={{ flex: 1 }}>
                <h3>{booking.hotelName}</h3>
                <p style={{ color: '#666', margin: '0.5rem 0' }}>
                  📍 {booking.hotelLocation}
                </p>
                <p style={{ color: '#666', margin: '0.5rem 0' }}>
                  <strong>Room:</strong> {booking.roomType}
                </p>
                <p style={{ color: '#666', margin: '0.5rem 0' }}>
                  <strong>Reference:</strong> {booking.bookingReference}
                </p>
                <p style={{ color: '#666', margin: '0.5rem 0' }}>
                  <strong>Dates:</strong> {new Date(booking.checkInDate).toLocaleDateString()} -{' '}
                  {new Date(booking.checkOutDate).toLocaleDateString()}
                </p>
                <p style={{ color: '#666', margin: '0.5rem 0' }}>
                  <strong>Guests:</strong> {booking.numberOfGuests} | <strong>Rooms:</strong>{' '}
                  {booking.numberOfRooms} | <strong>Nights:</strong> {booking.numberOfNights}
                </p>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div className="price">${booking.totalPrice}</div>
                <div className="price-label">Total</div>
                <div className="mt-2">
                  <span
                    className={`status-badge ${
                      booking.status === 'Confirmed'
                        ? 'status-confirmed'
                        : booking.status === 'Cancelled'
                        ? 'status-cancelled'
                        : 'status-pending'
                    }`}
                  >
                    {booking.status}
                  </span>
                </div>
                {booking.status === 'Confirmed' && canCancel(booking.checkInDate) && (
                  <button
                    onClick={() => handleCancelBooking(booking.bookingId)}
                    className="btn btn-danger mt-2"
                    style={{ width: '100%' }}
                  >
                    Cancel Booking
                  </button>
                )}
                {booking.status === 'Confirmed' && !canCancel(booking.checkInDate) && (
                  <p style={{ fontSize: '0.85rem', color: '#666', marginTop: '0.5rem' }}>
                    Cannot cancel (less than 24h)
                  </p>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MyBookings;
