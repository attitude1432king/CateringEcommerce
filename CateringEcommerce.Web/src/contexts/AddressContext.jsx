import { createContext, useContext, useState, useEffect } from 'react';
import { useAuth } from './AuthContext';
import { useAppSettings } from './AppSettingsContext';

const AddressContext = createContext(null);

export const useAddress = () => {
  const context = useContext(AddressContext);
  if (!context) {
    throw new Error('useAddress must be used within AddressProvider');
  }
  return context;
};

export const AddressProvider = ({ children }) => {
  const { user, token } = useAuth();
  const { getSetting, getInt } = useAppSettings();
  const [addresses, setAddresses] = useState([]);
  const [defaultAddress, setDefaultAddress] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const API_BASE_URL = getSetting('SYSTEM.API_BASE_URL', import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368') + '/api';

  // Fetch user's saved addresses
  const fetchAddresses = async () => {
    if (!user || !token) {
      setAddresses([]);
      setDefaultAddress(null);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/User/UserAddresses`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to fetch addresses');
      }

      const data = await response.json();
      setAddresses(data);

      // Find default address
      const defaultAddr = data.find(addr => addr.isDefault);
      setDefaultAddress(defaultAddr || null);
    } catch (err) {
      setError(err.message);
      console.error('Error fetching addresses:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Add new address
  const addAddress = async (addressData) => {
    if (!user || !token) {
      throw new Error('User not authenticated');
    }

    try {
      setIsLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/User/UserAddresses`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(addressData)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to add address');
      }

      const newAddress = await response.json();
      setAddresses(prev => [...prev, newAddress]);

      // If this is set as default, update default address
      if (newAddress.isDefault) {
        setDefaultAddress(newAddress);
      }

      return newAddress;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  // Update existing address
  const updateAddress = async (addressId, addressData) => {
    if (!user || !token) {
      throw new Error('User not authenticated');
    }

    try {
      setIsLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/User/UserAddresses/${addressId}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(addressData)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to update address');
      }

      const updatedAddress = await response.json();
      setAddresses(prev => prev.map(addr =>
        addr.addressId === addressId ? updatedAddress : addr
      ));

      // If this is set as default, update default address
      if (updatedAddress.isDefault) {
        setDefaultAddress(updatedAddress);
      }

      return updatedAddress;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  // Delete address
  const deleteAddress = async (addressId) => {
    if (!user || !token) {
      throw new Error('User not authenticated');
    }

    try {
      setIsLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/User/UserAddresses/${addressId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to delete address');
      }

      setAddresses(prev => prev.filter(addr => addr.addressId !== addressId));

      // If deleted address was default, clear default
      if (defaultAddress?.addressId === addressId) {
        setDefaultAddress(null);
      }

      return true;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  // Set address as default
  const setAsDefault = async (addressId) => {
    if (!user || !token) {
      throw new Error('User not authenticated');
    }

    try {
      setIsLoading(true);
      setError(null);

      const response = await fetch(`${API_BASE_URL}/User/UserAddresses/${addressId}/setDefault`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to set default address');
      }

      // Update local state
      setAddresses(prev => prev.map(addr => ({
        ...addr,
        isDefault: addr.addressId === addressId
      })));

      const newDefaultAddress = addresses.find(addr => addr.addressId === addressId);
      setDefaultAddress(newDefaultAddress);

      return true;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  // Get address by ID
  const getAddressById = (addressId) => {
    return addresses.find(addr => addr.addressId === addressId);
  };

  // Check if user can add more addresses
  const canAddAddress = () => {
    const maxAddresses = getInt('BUSINESS.MAX_ADDRESSES_PER_USER', 5);
    return addresses.length < maxAddresses;
  };

  // Fetch addresses when user logs in
  useEffect(() => {
    if (user && token) {
      fetchAddresses();
    } else {
      setAddresses([]);
      setDefaultAddress(null);
    }
  }, [user, token]);

  const value = {
    addresses,
    defaultAddress,
    isLoading,
    error,
    fetchAddresses,
    addAddress,
    updateAddress,
    deleteAddress,
    setAsDefault,
    getAddressById,
    canAddAddress
  };

  return (
    <AddressContext.Provider value={value}>
      {children}
    </AddressContext.Provider>
  );
};
