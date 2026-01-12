import React, { useState, useEffect } from 'react';
import {
  getHotelBookings,
  addRoomAvailability,
  getHotelAvailabilities,
  getNotificationStats,
  triggerLowCapacityCheck,
  triggerReservationProcessing,
} from '../services/api';
import LoadingSpinner from '../components/LoadingSpinner';

const AdminDashboard = () => {
  const [activeTab, setActiveTab] = useState('bookings');
  const [bookings, setBookings] = useState([]);
  const [availabilities, setAvailabilities] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(false);
  const [selectedHotel, setSelectedHotel] = useState(1);

  const [availabilityForm, setAvailabilityForm] = useState({
    hotelId: 1,
    roomId: 1,
    startDate: '',
    endDate: '',
    availableRooms: '',
    pricePerNight: '',
  });

  useEffect(() => {
    loadData();
  }, [activeTab, selectedHotel]);

  const loadData = async () => {
    setLoading(true);
    try {
      if (activeTab === 'bookings') {
        const data = await getHotelBookings(selectedHotel);
        setBookings(data);
      } else if (activeTab === 'availability') {
        const data = await getHotelAvailabilities(selectedHotel);
        setAvailabilities(data);
      } else if (activeTab === 'notifications') {
        const data = await getNotificationStats();
        setStats(data);
      }
    } catch (err) {
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddAvailability = async (e) => {
    e.preventDefault();
    try {
      await addRoomAvailability({
        ...availabilityForm,
        availableRooms: parseInt(availabilityForm.availableRooms),
        pricePerNight: parseFloat(availabilityForm.pricePerNight),
        startDate: new Date(availabilityForm.startDate).toISOString(),
        endDate: new Date(availabilityForm.endDate).toISOString(),
      });

      alert('Availability added successfully!');
      setAvailabilityForm({
        hotelId: selectedHotel,
        roomId: 1,
        startDate: '',
        endDate: '',
        availableRooms: '',
        pricePerNight: '',
      });
      loadData();
    } catch (err) {
      alert(err.error || err.message || 'Failed to add availability');
    }
  };

  const handleTriggerTask = async (task) => {
    try {
      if (task === 'capacity') {
        await triggerLowCapacityCheck();
        alert('Low capacity check triggered successfully!');
      } else {
        await triggerReservationProcessing();
        alert('Reservation processing triggered successfully!');
      }
    } catch (err) {
      alert('Failed to trigger task');
    }
  };

  return (
    <div>
      <div className="dashboard-header">
        <h1>Admin Dashboard</h1>
        <div className="flex gap-2">
          <select
            value={selectedHotel}
            onChange={(e) => setSelectedHotel(parseInt(e.target.value))}
            style={{ padding: '0.5rem 1rem' }}
          >
            <option value={1}>Grand Plaza Hotel</option>
            <option value={2}>Seaside Resort</option>
          </select>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-3">
        <button
          className={`btn ${activeTab === 'bookings' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('bookings')}
        >
          Bookings
        </button>
        <button
          className={`btn ${activeTab === 'availability' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('availability')}
        >
          Availability
        </button>
        <button
          className={`btn ${activeTab === 'notifications' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('notifications')}
        >
          Notifications
        </button>
      </div>

      {loading && <LoadingSpinner />}

      {/* Bookings Tab */}
      {!loading && activeTab === 'bookings' && (
        <div className="card">
          <h2>Hotel Bookings</h2>
          {bookings.length === 0 ? (
            <p>No bookings found</p>
          ) : (
            <div className="table">
              <table>
                <thead>
                  <tr>
                    <th>Reference</th>
                    <th>Guest</th>
                    <th>Room</th>
                    <th>Check-in</th>
                    <th>Check-out</th>
                    <th>Total</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {bookings.map((booking) => (
                    <tr key={booking.bookingId}>
                      <td>{booking.bookingReference}</td>
                      <td>
                        {booking.guestName}
                        <br />
                        <small style={{ color: '#666' }}>{booking.guestEmail}</small>
                      </td>
                      <td>{booking.roomType}</td>
                      <td>{new Date(booking.checkInDate).toLocaleDateString()}</td>
                      <td>{new Date(booking.checkOutDate).toLocaleDateString()}</td>
                      <td>${booking.totalPrice}</td>
                      <td>
                        <span
                          className={`status-badge ${
                            booking.status === 'Confirmed' ? 'status-confirmed' : 'status-cancelled'
                          }`}
                        >
                          {booking.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Availability Tab */}
      {!loading && activeTab === 'availability' && (
        <div>
          <div className="card mb-3">
            <h2>Add Room Availability</h2>
            <form onSubmit={handleAddAvailability}>
              <div className="grid grid-3">
                <div className="form-group">
                  <label>Room ID</label>
                  <select
                    value={availabilityForm.roomId}
                    onChange={(e) =>
                      setAvailabilityForm({ ...availabilityForm, roomId: parseInt(e.target.value) })
                    }
                    required
                  >
                    <option value={1}>Deluxe Suite</option>
                    <option value={2}>Standard Room</option>
                    {selectedHotel === 2 && <option value={3}>Ocean View Suite</option>}
                  </select>
                </div>

                <div className="form-group">
                  <label>Start Date</label>
                  <input
                    type="date"
                    value={availabilityForm.startDate}
                    onChange={(e) =>
                      setAvailabilityForm({ ...availabilityForm, startDate: e.target.value })
                    }
                    required
                  />
                </div>

                <div className="form-group">
                  <label>End Date</label>
                  <input
                    type="date"
                    value={availabilityForm.endDate}
                    onChange={(e) =>
                      setAvailabilityForm({ ...availabilityForm, endDate: e.target.value })
                    }
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Available Rooms</label>
                  <input
                    type="number"
                    min="0"
                    value={availabilityForm.availableRooms}
                    onChange={(e) =>
                      setAvailabilityForm({ ...availabilityForm, availableRooms: e.target.value })
                    }
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Price per Night</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={availabilityForm.pricePerNight}
                    onChange={(e) =>
                      setAvailabilityForm({ ...availabilityForm, pricePerNight: e.target.value })
                    }
                    required
                  />
                </div>
              </div>

              <button type="submit" className="btn btn-primary">
                Add Availability
              </button>
            </form>
          </div>

          <div className="card">
            <h2>Current Availabilities</h2>
            {availabilities.length === 0 ? (
              <p>No availabilities found</p>
            ) : (
              <div className="table">
                <table>
                  <thead>
                    <tr>
                      <th>Room Type</th>
                      <th>Start Date</th>
                      <th>End Date</th>
                      <th>Available</th>
                      <th>Price/Night</th>
                    </tr>
                  </thead>
                  <tbody>
                    {availabilities.map((avail) => (
                      <tr key={avail.id}>
                        <td>{avail.roomType}</td>
                        <td>{new Date(avail.startDate).toLocaleDateString()}</td>
                        <td>{new Date(avail.endDate).toLocaleDateString()}</td>
                        <td>{avail.availableRooms} rooms</td>
                        <td>${avail.pricePerNight}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Notifications Tab */}
      {!loading && activeTab === 'notifications' && (
        <div>
          <div className="dashboard-stats mb-3">
            <div className="stat-card">
              <div className="stat-label">Pending Notifications</div>
              <div className="stat-value">{stats?.totalPending || 0}</div>
            </div>
          </div>

          <div className="grid grid-2">
            <div className="card">
              <h3>Low Capacity Check</h3>
              <p>Trigger nightly capacity monitoring task</p>
              <button
                onClick={() => handleTriggerTask('capacity')}
                className="btn btn-primary mt-2"
              >
                Trigger Now
              </button>
            </div>

            <div className="card">
              <h3>Reservation Processing</h3>
              <p>Process pending booking confirmations</p>
              <button
                onClick={() => handleTriggerTask('reservations')}
                className="btn btn-primary mt-2"
              >
                Trigger Now
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDashboard;
