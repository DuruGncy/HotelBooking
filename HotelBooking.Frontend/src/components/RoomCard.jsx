import React from 'react';
import { useNavigate } from 'react-router-dom';

const RoomCard = ({ room, hotel, searchParams }) => {
  const navigate = useNavigate();

  const handleBookNow = () => {
    navigate('/book', {
      state: {
        hotel,
        room,
        searchParams,
      },
    });
  };

  const numberOfNights = searchParams
    ? Math.ceil(
        (new Date(searchParams.checkOutDate) - new Date(searchParams.checkInDate)) /
          (1000 * 60 * 60 * 24)
      )
    : 1;

  const totalPrice = room.pricePerNight * numberOfNights * (searchParams?.numberOfRooms || 1);

  return (
    <div className="card">
      <div className="flex-between">
        <div style={{ flex: 1 }}>
          <h3>{room.roomType}</h3>
          <p style={{ color: '#666', margin: '0.5rem 0' }}>
            <strong>Max Occupancy:</strong> {room.maxOccupancy} guests
          </p>
          <p style={{ color: '#666', margin: '0.5rem 0' }}>
            <strong>Amenities:</strong> {room.amenities?.join(', ') || 'N/A'}
          </p>
          <p style={{ color: '#666', margin: '0.5rem 0' }}>
            <strong>Available:</strong> {room.availableCount} rooms
          </p>
        </div>
        <div style={{ textAlign: 'right' }}>
          <div className="price">${room.pricePerNight}</div>
          <div className="price-label">per night</div>
          {searchParams && (
            <>
              <div style={{ marginTop: '1rem', fontSize: '1.2rem', fontWeight: 'bold' }}>
                ${totalPrice}
              </div>
              <div className="price-label">
                for {numberOfNights} nights
              </div>
            </>
          )}
          <button
            onClick={handleBookNow}
            className="btn btn-primary mt-2"
            disabled={room.availableCount < (searchParams?.numberOfRooms || 1)}
          >
            {room.availableCount < (searchParams?.numberOfRooms || 1)
              ? 'Not Available'
              : 'Book Now'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default RoomCard;
