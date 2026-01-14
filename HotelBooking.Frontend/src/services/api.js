import axios from 'axios';

// API Configuration - Direct connections to individual services
const CLIENT_API_URL = 'http://localhost:7021/api/v1.0';
const ADMIN_API_URL =  'http://localhost:7266/api/v1.0';
const NOTIFICATION_API_URL = process.env.REACT_APP_NOTIFICATION_API_URL || 'http://localhost:15002/api/v1.0';
const PREDICT_API_URL = process.env.REACT_APP_PREDICT_API_URL || 'http://localhost:8087/api/v1';

// Create axios instances for each service
const clientApi = axios.create({
  baseURL: CLIENT_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const adminApi = axios.create({
  baseURL: ADMIN_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const notificationApi = axios.create({
  baseURL: NOTIFICATION_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const predictApi = axios.create({
  baseURL: PREDICT_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Common interceptor setup function
const setupInterceptors = (apiInstance) => {
  // Request interceptor to add auth token
  apiInstance.interceptors.request.use(
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
  apiInstance.interceptors.response.use(
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
};

// Setup interceptors for all API instances
setupInterceptors(clientApi);
setupInterceptors(adminApi);
setupInterceptors(notificationApi);
setupInterceptors(predictApi);

// HOTEL SEARCH API (Client API)
export const searchHotels = async (searchParams) => {
  try {
    const response = await clientApi.post('/HotelSearch/search', searchParams);
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
    const response = await clientApi.get(`/HotelSearch/hotels/${hotelId}?${params}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// BOOKING API (Client API)
export const createBooking = async (bookingData) => {
  try {
    const response = await clientApi.post('/BookHotel/book', bookingData);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getBookingByReference = async (reference) => {
  try {
    const response = await clientApi.get(`/BookHotel/reference/${reference}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const cancelBooking = async (bookingId, reason) => {
  try {
    const response = await clientApi.post(`/BookHotel/${bookingId}/cancel`, {
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
    const response = await adminApi.post('/Admin/room-availability', availabilityData);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getHotelAvailabilities = async (hotelId) => {
  try {
    const response = await adminApi.get(`/Admin/hotels/${hotelId}/room-availabilities`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const getHotelBookings = async (hotelId) => {
  try {
    const response = await clientApi.get(`/BookHotel/hotel/${hotelId}`);
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// NOTIFICATION API
export const getNotificationStats = async () => {
  try {
    const response = await notificationApi.get('/Notifications/pending');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const triggerLowCapacityCheck = async () => {
  try {
    const response = await notificationApi.post('/Notifications/trigger/low-capacity-check');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

export const triggerReservationProcessing = async () => {
  try {
    const response = await notificationApi.post('/Notifications/trigger/process-reservations');
    return response.data;
  } catch (error) {
    throw error.response?.data || error.message;
  }
};

// PREDICT API (ML service)
export const predictPrice = async (predictPayload) => {
  try {
    const response = await predictApi.post('/pricing/predict', predictPayload);
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

export default clientApi;
