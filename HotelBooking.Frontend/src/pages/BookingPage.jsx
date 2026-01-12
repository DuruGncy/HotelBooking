import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { createBooking } from '../services/api';

const BookingPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { hotel, room, searchParams } = location.state || {};

  const [formData, setFormData] = useState({
    guestName: '',
    guestEmail: '',
    guestPhone: '',
    specialRequests: '',
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  if (!hotel || !room || !searchParams) {
    return (
      <div className="alert alert-error">
        <h3>Invalid Booking</h3>
        <p>Missing booking information. Please start from hotel search.</p>
      </div>
    );
  }

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const bookingData = {
        hotelId: hotel.hotelId,
        roomId: room.roomId,
        checkInDate: searchParams.checkInDate,
        checkOutDate: searchParams.checkOutDate,
        numberOfRooms: searchParams.numberOfRooms,
        numberOfGuests: searchParams.numberOfGuests,
        guestName: formData.guestName,
        guestEmail: formData.guestEmail,
        guestPhone: formData.guestPhone,
        specialRequests: formData.specialRequests || null,
      };

      const result = await createBooking(bookingData);
      navigate(`/confirmation/${result.bookingReference}`, { state: { booking: result } });
    } catch (err) {
      setError(err.error || err.message || 'Failed to create booking');
    } finally {
      setLoading(false);
    }
  };

  const numberOfNights = Math.ceil(
    (new Date(searchParams.checkOutDate) - new Date(searchParams.checkInDate)) /
      (1000 * 60 * 60 * 24)
  );
  const totalPrice = room.pricePerNight * numberOfNights * searchParams.numberOfRooms;

  return (
    <div className="grid grid-2">
      {/* Booking Form */}
      <div>
        <div className="card">
          <h2>Guest Information</h2>
          {error && (
            <div className="alert alert-error">
              <strong>Error:</strong> {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="guestName">Full Name *</label>
              <input
                type="text"
                id="guestName"
                name="guestName"
                value={formData.guestName}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="guestEmail">Email Address *</label>
              <input
                type="email"
                id="guestEmail"
                name="guestEmail"
                value={formData.guestEmail}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="guestPhone">Phone Number *</label>
              <input
                type="tel"
                id="guestPhone"
                name="guestPhone"
                value={formData.guestPhone}
                onChange={handleChange}
                placeholder="+1-555-0123"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="specialRequests">Special Requests</label>
              <textarea
                id="specialRequests"
                name="specialRequests"
                value={formData.specialRequests}
                onChange={handleChange}
                rows="4"
                placeholder="Any special requirements or requests..."
              ></textarea>
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%' }}
              disabled={loading}
            >
              {loading ? 'Processing...' : 'Confirm Booking'}
            </button>
          </form>
        </div>
      </div>

      {/* Booking Summary */}
      <div>
        <div className="card">
          <h2>Booking Summary</h2>
          <div className="booking-summary">
            <h3>{hotel.hotelName}</h3>
            <p>📍 {hotel.location}</p>
            <p>{'⭐'.repeat(hotel.starRating)}</p>

            <hr style={{ margin: '1rem 0' }} />

            <h4>{room.roomType}</h4>
            <p style={{ color: '#666' }}>{room.amenities?.join(', ')}</p>

            <hr style={{ margin: '1rem 0' }} />

            <div className="summary-row">
              <span>Check-in:</span>
              <strong>{new Date(searchParams.checkInDate).toLocaleDateString()}</strong>
            </div>

            <div className="summary-row">
              <span>Check-out:</span>
              <strong>{new Date(searchParams.checkOutDate).toLocaleDateString()}</strong>
            </div>

            <div className="summary-row">
              <span>Number of Nights:</span>
              <strong>{numberOfNights}</strong>
            </div>

            <div className="summary-row">
              <span>Number of Rooms:</span>
              <strong>{searchParams.numberOfRooms}</strong>
            </div>

            <div className="summary-row">
              <span>Number of Guests:</span>
              <strong>{searchParams.numberOfGuests}</strong>
            </div>

            <hr style={{ margin: '1rem 0' }} />

            <div className="summary-row">
              <span>Price per night:</span>
              <strong>${room.pricePerNight}</strong>
            </div>

            <div className="summary-row" style={{ fontSize: '1.3rem', color: '#667eea' }}>
              <span>Total Price:</span>
              <strong>${totalPrice}</strong>
            </div>
          </div>
        </div>

        <div className="alert alert-info mt-2">
          <strong>Important:</strong>
          <ul style={{ marginTop: '0.5rem', paddingLeft: '1.5rem' }}>
            <li>Free cancellation up to 24 hours before check-in</li>
            <li>Confirmation email will be sent instantly</li>
            <li>Save your booking reference number</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default BookingPage;
