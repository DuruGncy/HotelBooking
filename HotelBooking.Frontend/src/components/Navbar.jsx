import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { isAuthenticated, isAdmin, removeAuthToken } from '../services/api';

const Navbar = () => {
  const navigate = useNavigate();
  const authenticated = isAuthenticated();
  const adminUser = isAdmin();

  const handleLogout = () => {
    removeAuthToken();
    navigate('/');
    window.location.reload();
  };

  return (
    <nav className="navbar">
      <div className="navbar-content">
        <Link to="/" className="navbar-brand">
          Duru's Hotel Booking
        </Link>
        <ul className="navbar-links">
          <li><Link to="/">Home</Link></li>
          <li><Link to="/search">Search</Link></li>
          <li><Link to="/my-bookings">My Bookings</Link></li>
          
          {authenticated && (
            <>
              {adminUser && (
                <li><Link to="/admin">Admin</Link></li>
              )}
              <li><button onClick={handleLogout}>Logout</button></li>
            </>
          )}
          
          {!authenticated && (
            <li><Link to="/login">Login</Link></li>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
