import React, { useState, useEffect } from 'react';
import { useParams, useLocation } from 'react-router-dom';
import { getHotelDetails } from '../services/api';
import RoomCard from '../components/RoomCard';
import LoadingSpinner from '../components/LoadingSpinner';

const HotelDetails = () => {
  const { hotelId } = useParams();
  const location = useLocation();
  const { hotel: initialHotel, searchParams } = location.state || {};

  const [hotel, setHotel] = useState(initialHotel || null);
  const [loading, setLoading] = useState(!initialHotel);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!initialHotel && searchParams) {
      const fetchHotelDetails = async () => {
        try {
          setLoading(true);
          const data = await getHotelDetails(
            hotelId,
            searchParams.checkInDate,
            searchParams.checkOutDate,
            searchParams.numberOfGuests
          );
          setHotel(data);
        } catch (err) {
          setError(err.message || 'Failed to fetch hotel details');
        } finally {
          setLoading(false);
        }
      };

      fetchHotelDetails();
    }
  }, [hotelId, initialHotel, searchParams]);

  if (loading) {
    return <LoadingSpinner message="Loading hotel details..." />;
  }

  if (error) {
    return (
      <div className="alert alert-error">
        <h3>Error</h3>
        <p>{error}</p>
      </div>
    );
  }

  if (!hotel) {
    return (
      <div className="card text-center">
        <h2>Hotel not found</h2>
      </div>
    );
  }

  return (
    <div>
      <div className="card">
        <div className="flex-between">
          <div>
            <h1>{hotel.hotelName}</h1>
            <div className="hotel-location mt-1">
              📍 {hotel.location}
            </div>
            <div className="hotel-rating mt-1">
              {'⭐'.repeat(hotel.starRating)}
            </div>
          </div>
          <div className="hotel-card-image" style={{ width: '200px', height: '150px' }}>
            🏨
          </div>
        </div>
      </div>

      {searchParams && (
        <div className="alert alert-info mt-2">
          <strong>Your Search:</strong>
          <br />
          Check-in: {new Date(searchParams.checkInDate).toLocaleDateString()} |
          Check-out: {new Date(searchParams.checkOutDate).toLocaleDateString()} |
          {searchParams.numberOfGuests} guests | {searchParams.numberOfRooms} room(s)
        </div>
      )}

      <h2 className="mt-3 mb-2">Available Rooms</h2>
      {hotel.availableRooms && hotel.availableRooms.length > 0 ? (
        <div className="grid grid-2">
          {hotel.availableRooms.map((room) => (
            <RoomCard
              key={room.roomId}
              room={room}
              hotel={hotel}
              searchParams={searchParams}
            />
          ))}
        </div>
      ) : (
        <div className="card text-center">
          <p>No rooms available for your selected dates</p>
        </div>
      )}
    </div>
  );
};

export default HotelDetails;
