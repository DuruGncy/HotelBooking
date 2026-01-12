import React, { useState, useEffect } from 'react';
import { useParams, useLocation, Link } from 'react-router-dom';
import { getBookingByReference } from '../services/api';
import LoadingSpinner from '../components/LoadingSpinner';

const BookingConfirmation = () => {
  const { bookingReference } = useParams();
  const location = useLocation();
  const initialBooking = location.state?.booking;

  const [booking, setBooking] = useState(initialBooking || null);
  const [loading, setLoading] = useState(!initialBooking);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!initialBooking) {
      const fetchBooking = async () => {
        try {
          setLoading(true);
          const data = await getBookingByReference(bookingReference);
          setBooking(data);
        } catch (err) {
          setError(err.message || 'Failed to fetch booking details');
        } finally {
          setLoading(false);
        }
      };

      fetchBooking();
    }
  }, [bookingReference, initialBooking]);

  if (loading) {
    return <LoadingSpinner message="Loading booking details..." />;
  }

  if (error) {
    return (
      <div className="alert alert-error">
        <h3>Error</h3>
        <p>{error}</p>
      </div>
    );
  }

  if (!booking) {
    return (
      <div className="card text-center">
        <h2>Booking not found</h2>
      </div>
    );
  }

  return (
    <div className="text-center">
      <div className="card">
        <div style={{ fontSize: '4rem', color: '#28a745', marginBottom: '1rem' }}>
          ✅
        </div>
        <h1>Booking Confirmed!</h1>
        <p style={{ fontSize: '1.2rem', color: '#666', marginTop: '1rem' }}>
          Your reservation has been successfully created
        </p>

        <div className="alert alert-success mt-3">
          <h2>Booking Reference</h2>
          <h1 style={{ color: '#667eea', margin: '1rem 0' }}>
            {booking.bookingReference}
          </h1>
          <p>Save this reference number for your records</p>
        </div>

        <div className="booking-summary mt-3" style={{ textAlign: 'left' }}>
          <h3>Booking Details</h3>

          <div className="summary-row">
            <span>Hotel:</span>
            <strong>{booking.hotelName}</strong>
          </div>

          <div className="summary-row">
            <span>Location:</span>
            <strong>{booking.hotelLocation}</strong>
          </div>

          <div className="summary-row">
            <span>Room Type:</span>
            <strong>{booking.roomType}</strong>
          </div>

          <hr style={{ margin: '1rem 0' }} />

          <div className="summary-row">
            <span>Check-in:</span>
            <strong>{new Date(booking.checkInDate).toLocaleDateString()}</strong>
          </div>

          <div className="summary-row">
            <span>Check-out:</span>
            <strong>{new Date(booking.checkOutDate).toLocaleDateString()}</strong>
          </div>

          <div className="summary-row">
            <span>Number of Nights:</span>
            <strong>{booking.numberOfNights}</strong>
          </div>

          <div className="summary-row">
            <span>Number of Rooms:</span>
            <strong>{booking.numberOfRooms}</strong>
          </div>

          <div className="summary-row">
            <span>Number of Guests:</span>
            <strong>{booking.numberOfGuests}</strong>
          </div>

          <hr style={{ margin: '1rem 0' }} />

          <div className="summary-row">
            <span>Guest Name:</span>
            <strong>{booking.guestName}</strong>
          </div>

          <div className="summary-row">
            <span>Email:</span>
            <strong>{booking.guestEmail}</strong>
          </div>

          <div className="summary-row">
            <span>Phone:</span>
            <strong>{booking.guestPhone}</strong>
          </div>

          <hr style={{ margin: '1rem 0' }} />

          <div className="summary-row" style={{ fontSize: '1.3rem', color: '#667eea' }}>
            <span>Total Price:</span>
            <strong>${booking.totalPrice}</strong>
          </div>

          <div className="summary-row">
            <span>Status:</span>
            <span className="status-badge status-confirmed">{booking.status}</span>
          </div>
        </div>

        <div className="alert alert-info mt-3">
          <p><strong>📧 Confirmation Email Sent</strong></p>
          <p>A confirmation email has been sent to {booking.guestEmail}</p>
        </div>

        <div className="flex gap-2 mt-3" style={{ justifyContent: 'center' }}>
          <Link to="/" className="btn btn-secondary">
            Back to Home
          </Link>
          <Link to="/my-bookings" className="btn btn-primary">
            View All Bookings
          </Link>
        </div>
      </div>
    </div>
  );
};

export default BookingConfirmation;
