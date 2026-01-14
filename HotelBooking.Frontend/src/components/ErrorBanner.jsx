import React from 'react';

const ErrorBanner = ({ title = 'HOTEL ADMIN SERVICE', message, onClose }) => {
  return (
    <div style={{ border: '2px solid #ccc', padding: '1rem', marginBottom: '1rem', background: '#f7f7f7' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ margin: 0, color: '#2c3e50' }}>{title}</h3>
        {onClose && (
          <button onClick={onClose} style={{ border: 'none', background: '#fff', cursor: 'pointer' }}>
            ✕
          </button>
        )}
      </div>
      {message && (
        <div style={{ marginTop: '0.75rem', color: '#333' }}>{message}</div>
      )}
    </div>
  );
};

export default ErrorBanner;
