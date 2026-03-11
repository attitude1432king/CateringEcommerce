import React, { useState, useEffect } from 'react';
import { useAddress } from '../../contexts/AddressContext';

/**
 * My Addresses Component
 * Displays and manages user's saved addresses
 * Uses AddressContext for state management
 */
const MyAddresses = () => {
  const {
    addresses,
    defaultAddress,
    isLoading,
    error,
    fetchAddresses,
    addAddress,
    updateAddress,
    deleteAddress,
    setDefaultAddress: setDefault
  } = useAddress();

  const [showAddForm, setShowAddForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [formData, setFormData] = useState({
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    pincode: '',
    addressType: 'Home',
    isDefault: false
  });

  useEffect(() => {
    fetchAddresses();
  }, []);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      if (editingId) {
        await updateAddress(editingId, formData);
      } else {
        await addAddress(formData);
      }

      // Reset form
      setFormData({
        addressLine1: '',
        addressLine2: '',
        city: '',
        state: '',
        pincode: '',
        addressType: 'Home',
        isDefault: false
      });
      setShowAddForm(false);
      setEditingId(null);
    } catch (err) {
      console.error('Error saving address:', err);
      alert('Failed to save address. Please try again.');
    }
  };

  const handleEdit = (address) => {
    setFormData({
      addressLine1: address.addressLine1,
      addressLine2: address.addressLine2 || '',
      city: address.city,
      state: address.state,
      pincode: address.pincode,
      addressType: address.addressType || 'Home',
      isDefault: address.isDefault || false
    });
    setEditingId(address.addressId);
    setShowAddForm(true);
  };

  const handleDelete = async (addressId) => {
    if (!confirm('Are you sure you want to delete this address?')) return;

    try {
      await deleteAddress(addressId);
    } catch (err) {
      console.error('Error deleting address:', err);
      alert('Failed to delete address. Please try again.');
    }
  };

  const handleSetDefault = async (addressId) => {
    try {
      await setDefault(addressId);
    } catch (err) {
      console.error('Error setting default address:', err);
      alert('Failed to set default address. Please try again.');
    }
  };

  if (isLoading && addresses.length === 0) {
    return (
      <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px] flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto mb-4"></div>
          <p className="text-neutral-600">Loading addresses...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 bg-white rounded-lg shadow-md min-h-[400px]">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-neutral-800">My Addresses</h2>
        <button
          onClick={() => {
            setShowAddForm(!showAddForm);
            setEditingId(null);
            setFormData({
              addressLine1: '',
              addressLine2: '',
              city: '',
              state: '',
              pincode: '',
              addressType: 'Home',
              isDefault: false
            });
          }}
          className="px-4 py-2 bg-rose-600 text-white rounded-lg hover:bg-rose-700 transition"
        >
          {showAddForm ? 'Cancel' : '+ Add New Address'}
        </button>
      </div>

      {/* Error Message */}
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      {/* Add/Edit Form */}
      {showAddForm && (
        <form onSubmit={handleSubmit} className="mb-6 p-6 bg-neutral-50 rounded-lg border border-neutral-200">
          <h3 className="text-lg font-semibold text-neutral-800 mb-4">
            {editingId ? 'Edit Address' : 'Add New Address'}
          </h3>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Address Line 1 *
              </label>
              <input
                type="text"
                name="addressLine1"
                value={formData.addressLine1}
                onChange={handleInputChange}
                required
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                placeholder="House/Flat No., Building Name"
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Address Line 2
              </label>
              <input
                type="text"
                name="addressLine2"
                value={formData.addressLine2}
                onChange={handleInputChange}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                placeholder="Street, Locality"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                City *
              </label>
              <input
                type="text"
                name="city"
                value={formData.city}
                onChange={handleInputChange}
                required
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                State *
              </label>
              <input
                type="text"
                name="state"
                value={formData.state}
                onChange={handleInputChange}
                required
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Pincode *
              </label>
              <input
                type="text"
                name="pincode"
                value={formData.pincode}
                onChange={handleInputChange}
                required
                pattern="[0-9]{6}"
                maxLength="6"
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                placeholder="6-digit pincode"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Address Type
              </label>
              <select
                name="addressType"
                value={formData.addressType}
                onChange={handleInputChange}
                className="w-full px-4 py-2 border border-neutral-300 rounded-lg focus:ring-2 focus:ring-rose-500 focus:border-transparent"
              >
                <option value="Home">Home</option>
                <option value="Work">Work</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div className="mt-4">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                name="isDefault"
                checked={formData.isDefault}
                onChange={handleInputChange}
                className="w-4 h-4 text-rose-600 border-neutral-300 rounded focus:ring-rose-500"
              />
              <span className="text-sm text-neutral-700">Set as default address</span>
            </label>
          </div>

          <div className="flex gap-3 mt-6">
            <button
              type="submit"
              className="px-6 py-2 bg-rose-600 text-white rounded-lg hover:bg-rose-700 transition"
            >
              {editingId ? 'Update Address' : 'Save Address'}
            </button>
            <button
              type="button"
              onClick={() => {
                setShowAddForm(false);
                setEditingId(null);
              }}
              className="px-6 py-2 bg-neutral-200 text-neutral-700 rounded-lg hover:bg-neutral-300 transition"
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      {/* Addresses List */}
      <div className="space-y-4">
        {addresses.length === 0 ? (
          <div className="text-center py-12">
            <svg className="h-16 w-16 text-neutral-400 mx-auto mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <h3 className="text-lg font-semibold text-neutral-800 mb-2">No Addresses Saved</h3>
            <p className="text-neutral-600 mb-4">Add your first address to make checkout faster.</p>
          </div>
        ) : (
          addresses.map((address) => (
            <div
              key={address.addressId}
              className={`p-4 rounded-lg border-2 transition ${
                address.isDefault || address.addressId === defaultAddress?.addressId
                  ? 'border-rose-500 bg-rose-50'
                  : 'border-neutral-200 bg-white'
              }`}
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="px-3 py-1 bg-neutral-100 text-neutral-700 text-xs font-medium rounded-full">
                      {address.addressType || 'Home'}
                    </span>
                    {(address.isDefault || address.addressId === defaultAddress?.addressId) && (
                      <span className="px-3 py-1 bg-rose-600 text-white text-xs font-medium rounded-full">
                        Default
                      </span>
                    )}
                  </div>

                  <p className="text-neutral-800 font-medium">{address.addressLine1}</p>
                  {address.addressLine2 && (
                    <p className="text-neutral-600">{address.addressLine2}</p>
                  )}
                  <p className="text-neutral-600">
                    {address.city}, {address.state} - {address.pincode}
                  </p>
                </div>

                <div className="flex gap-2 ml-4">
                  <button
                    onClick={() => handleEdit(address)}
                    className="p-2 text-neutral-600 hover:text-rose-600 transition"
                    title="Edit"
                  >
                    <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>

                  {!(address.isDefault || address.addressId === defaultAddress?.addressId) && (
                    <>
                      <button
                        onClick={() => handleSetDefault(address.addressId)}
                        className="p-2 text-neutral-600 hover:text-green-600 transition"
                        title="Set as Default"
                      >
                        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                      </button>

                      <button
                        onClick={() => handleDelete(address.addressId)}
                        className="p-2 text-neutral-600 hover:text-red-600 transition"
                        title="Delete"
                      >
                        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default MyAddresses;
