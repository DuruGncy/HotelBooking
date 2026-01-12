import React from 'react';
import { useNavigate } from 'react-router-dom';

const HotelCard = ({ hotel, searchParams }) => {
  const navigate = useNavigate();

  const handleViewDetails = () => {
    navigate(`/hotel/${hotel.hotelId}`, { state: { hotel, searchParams } });
  };

  return (
    <div className="hotel-card">
      <div className="hotel-card-image">
        ??
      </div>
      <div className="hotel-card-content">
        <div className="hotel-card-header">
          <h3 className="hotel-card-title">{hotel.hotelName}</h3>
          <div className="hotel-rating">
            {'?'.repeat(hotel.starRating)}
          </div>
        </div>

        <div className="hotel-location">
          ?? {hotel.location}
        </div>

        <div className="room-list">
          {hotel.availableRooms && hotel.availableRooms.slice(0, 2).map((room) => (
            <div key={room.roomId} className="room-item">
              <div className="flex-between">
                <div>
                  <strong>{room.roomType}</strong>
                  <p style={{ fontSize: '0.9rem', color: '#666', margin: '0.25rem 0' }}>
                    {room.amenities?.join(', ')}
                  </p>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div className="price">${room.pricePerNight}</div>
                  <div className="price-label">per night</div>
                </div>
              </div>
            </div>
          ))}
        </div>

        <div className="flex-between mt-2">
          <div>
            <div className="price">${hotel.lowestPricePerNight}</div>
            <div className="price-label">Starting from</div>
          </div>
          <button onClick={handleViewDetails} className="btn btn-primary">
            View Details
          </button>
        </div>
      </div>
    </div>
  );
};

export default HotelCard;
