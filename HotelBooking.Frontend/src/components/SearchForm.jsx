import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';

const SearchForm = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    destination: '',
    checkInDate: '',
    checkOutDate: '',
    numberOfGuests: 2,
    numberOfRooms: 1,
  });

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    
    const searchParams = {
      ...formData,
      checkInDate: new Date(formData.checkInDate).toISOString(),
      checkOutDate: new Date(formData.checkOutDate).toISOString(),
      numberOfGuests: parseInt(formData.numberOfGuests),
      numberOfRooms: parseInt(formData.numberOfRooms),
    };

    navigate('/search', { state: { searchParams } });
  };

  const today = format(new Date(), 'yyyy-MM-dd');

  return (
    <form onSubmit={handleSubmit} className="search-form">
      <div className="search-form-grid">
        <div className="form-group">
          <label htmlFor="destination">Destination</label>
          <input
            type="text"
            id="destination"
            name="destination"
            value={formData.destination}
            onChange={handleChange}
            placeholder="e.g., New York"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="checkInDate">Check-in</label>
          <input
            type="date"
            id="checkInDate"
            name="checkInDate"
            value={formData.checkInDate}
            onChange={handleChange}
            min={today}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="checkOutDate">Check-out</label>
          <input
            type="date"
            id="checkOutDate"
            name="checkOutDate"
            value={formData.checkOutDate}
            onChange={handleChange}
            min={formData.checkInDate || today}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="numberOfGuests">Guests</label>
          <select
            id="numberOfGuests"
            name="numberOfGuests"
            value={formData.numberOfGuests}
            onChange={handleChange}
            required
          >
            {[1, 2, 3, 4, 5, 6].map((num) => (
              <option key={num} value={num}>
                {num} {num === 1 ? 'Guest' : 'Guests'}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="numberOfRooms">Rooms</label>
          <select
            id="numberOfRooms"
            name="numberOfRooms"
            value={formData.numberOfRooms}
            onChange={handleChange}
            required
          >
            {[1, 2, 3, 4, 5].map((num) => (
              <option key={num} value={num}>
                {num} {num === 1 ? 'Room' : 'Rooms'}
              </option>
            ))}
          </select>
        </div>
      </div>

      <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
        Search Hotels
      </button>
    </form>
  );
};

export default SearchForm;
