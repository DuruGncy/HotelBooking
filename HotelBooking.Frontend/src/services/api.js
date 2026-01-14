import axios from 'axios';

// API Configuration
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7001/api';
const API_VERSION = process.env.REACT_APP_API_VERSION || 'v1.0';

// Create axios instance
const api = axios.create({
  baseURL: `${API_BASE_URL}/${API_VERSION}`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('authToken');
      localStorage.removeItem('userRole');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// HOTEL SEARCH API
export const searchHotels = async (searchParams) => {
  try {
    const response = await api.post('/HotelSearch/search', searchParams);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getHotelDetails = async (hotelId, checkInDate, checkOutDate, numberOfGuests) => {
  try {
    const params = new URLSearchParams({
      checkInDate,
      checkOutDate,
      numberOfGuests: numberOfGuests.toString(),
    });
    const response = await api.get(`/HotelSearch/hotels/${hotelId}?${params}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// BOOKING API
export const createBooking = async (bookingData) => {
  try {
    const response = await api.post('/BookHotel/book', bookingData);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getBookingByReference = async (reference) => {
  try {
    const response = await api.get(`/BookHotel/reference/${reference}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const cancelBooking = async (bookingId, reason) => {
  try {
    const response = await api.post(`/BookHotel/${bookingId}/cancel`, {
      cancellationReason: reason,
    });
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// ADMIN API
export const addRoomAvailability = async (availabilityData) => {
  try {
    const response = await api.post('/AdminHotels/room-availability', availabilityData);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getHotelAvailabilities = async (hotelId) => {
  try {
    const response = await api.get(`/AdminHotels/hotels/${hotelId}/room-availabilities`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getHotelBookings = async (hotelId) => {
  try {
    const response = await api.get(`/BookHotel/hotel/${hotelId}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// NOTIFICATION API
export const getNotificationStats = async () => {
  try {
    const response = await api.get('/Notification/stats');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const triggerLowCapacityCheck = async () => {
  try {
    const response = await api.post('/Notification/trigger/low-capacity-check');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const triggerReservationProcessing = async () => {
  try {
    const response = await api.post('/Notification/trigger/process-reservations');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// PREDICT API (direct call to ML service)
export const predictPrice = async (predictPayload) => {
  try {
    const PREDICT_BASE = process.env.REACT_APP_PREDICT_API_URL || 'http://localhost:8087/api';
    const PREDICT_VER = process.env.REACT_APP_PREDICT_API_VERSION || 'v1';
    const url = `${PREDICT_BASE}/${PREDICT_VER}/pricing/predict`;
    const response = await axios.post(url, predictPayload, {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// AUTH HELPERS
export const setAuthToken = (token) => {
  localStorage.setItem('authToken', token);
};

export const removeAuthToken = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('userRole');
};

export const getAuthToken = () => {
  return localStorage.getItem('authToken');
};

export const setUserRole = (role) => {
  localStorage.setItem('userRole', role);
};

export const getUserRole = () => {
  return localStorage.getItem('userRole');
};

export const isAdmin = () => {
  return getUserRole() === 'ADMIN';
};

export const isAuthenticated = () => {
  return !!getAuthToken();
};

export default api;
