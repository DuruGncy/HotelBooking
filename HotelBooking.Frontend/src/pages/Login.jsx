import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { setAuthToken, setUserRole } from '../services/api';

const Login = () => {
  const navigate = useNavigate();
  const [role, setRole] = useState('USER');

  const handleLogin = (e) => {
    e.preventDefault();

    // Demo: Generate a mock token
    // In production, call your auth API endpoint
    const mockToken = 'demo-jwt-token-' + Date.now();

    setAuthToken(mockToken);
    setUserRole(role);

    alert(`Logged in as ${role}`);
    navigate('/');
    window.location.reload();
  };

  return (
    <div className="flex-center" style={{ minHeight: '60vh' }}>
      <div className="card" style={{ maxWidth: '400px', width: '100%' }}>
        <h2 className="text-center">Login</h2>
        <p className="text-center" style={{ color: '#666', marginBottom: '2rem' }}>
          Demo login - select your role
        </p>

        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label>Select Role</label>
            <select value={role} onChange={(e) => setRole(e.target.value)}>
              <option value="USER">User</option>
              <option value="ADMIN">Admin</option>
            </select>
          </div>

          <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
            Login
          </button>
        </form>

        <div className="alert alert-info mt-3">
          <strong>Demo Mode:</strong>
          <ul style={{ marginTop: '0.5rem', paddingLeft: '1.5rem' }}>
            <li>No password required</li>
            <li>Select USER for booking features</li>
            <li>Select ADMIN for dashboard access</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Login;
