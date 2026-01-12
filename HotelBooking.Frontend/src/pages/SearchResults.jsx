import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { searchHotels } from '../services/api';
import HotelCard from '../components/HotelCard';
import LoadingSpinner from '../components/LoadingSpinner';

const SearchResults = () => {
  const location = useLocation();
  const searchParams = location.state?.searchParams;

  const [hotels, setHotels] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!searchParams) {
      setError('No search parameters provided');
      setLoading(false);
      return;
    }

    const fetchHotels = async () => {
      try {
        setLoading(true);
        setError(null);
        const results = await searchHotels(searchParams);
        setHotels(results);
      } catch (err) {
        setError(err.message || 'Failed to fetch hotels');
      } finally {
        setLoading(false);
      }
    };

    fetchHotels();
  }, [searchParams]);

  if (loading) {
    return <LoadingSpinner message="Searching for hotels..." />;
  }

  if (error) {
    return (
      <div className="alert alert-error">
        <h3>Error</h3>
        <p>{error}</p>
      </div>
    );
  }

  if (!hotels || hotels.length === 0) {
    return (
      <div className="card text-center">
        <h2>No hotels found</h2>
        <p>Try adjusting your search criteria</p>
      </div>
    );
  }

  return (
    <div>
      <h1 className="mb-2">
        Search Results
        {searchParams && ` - ${searchParams.destination}`}
      </h1>
      <p className="mb-3" style={{ color: '#666' }}>
        Found {hotels.length} {hotels.length === 1 ? 'hotel' : 'hotels'}
      </p>

      <div className="grid grid-2">
        {hotels.map((hotel) => (
          <HotelCard key={hotel.hotelId} hotel={hotel} searchParams={searchParams} />
        ))}
      </div>
    </div>
  );
};

export default SearchResults;
