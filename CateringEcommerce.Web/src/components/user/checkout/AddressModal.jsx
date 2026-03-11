import { useState, useEffect } from 'react';
import { useAddress } from '../../../contexts/AddressContext';

const AddressModal = ({ isOpen, onClose, editingAddress = null }) => {
  const { addAddress, updateAddress } = useAddress();
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const [formData, setFormData] = useState({
    addressLabel: 'Home',
    fullAddress: '',
    landmark: '',
    city: '',
    state: '',
    pincode: '',
    contactPerson: '',
    contactPhone: '',
    isDefault: false
  });

  // Populate form when editing
  useEffect(() => {
    if (editingAddress) {
      setFormData({
        addressLabel: editingAddress.addressLabel || 'Home',
        fullAddress: editingAddress.fullAddress || '',
        landmark: editingAddress.landmark || '',
        city: editingAddress.city || '',
        state: editingAddress.state || '',
        pincode: editingAddress.pincode || '',
        contactPerson: editingAddress.contactPerson || '',
        contactPhone: editingAddress.contactPhone || '',
        isDefault: editingAddress.isDefault || false
      });
    }
  }, [editingAddress]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    // Clear error for this field
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: null }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.fullAddress.trim()) {
      newErrors.fullAddress = 'Full address is required';
    }

    if (!formData.city.trim()) {
      newErrors.city = 'City is required';
    }

    if (!formData.state.trim()) {
      newErrors.state = 'State is required';
    }

    if (!formData.pincode.trim()) {
      newErrors.pincode = 'Pincode is required';
    } else if (!/^\d{6}$/.test(formData.pincode)) {
      newErrors.pincode = 'Pincode must be 6 digits';
    }

    if (!formData.contactPerson.trim()) {
      newErrors.contactPerson = 'Contact person name is required';
    }

    if (!formData.contactPhone.trim()) {
      newErrors.contactPhone = 'Contact phone is required';
    } else if (!/^\d{10}$/.test(formData.contactPhone)) {
      newErrors.contactPhone = 'Phone must be 10 digits';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      setIsLoading(true);

      if (editingAddress) {
        await updateAddress(editingAddress.addressId, formData);
      } else {
        await addAddress(formData);
      }

      // Success - close modal
      onClose();
    } catch (error) {
      alert(error.message || 'Failed to save address. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onClick={onClose}></div>

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-lg shadow-xl w-full max-w-2xl">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <h2 className="text-2xl font-bold text-gray-900">
              {editingAddress ? 'Edit Address' : 'Add New Address'}
            </h2>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="p-6 space-y-4">
            {/* Address Label */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Address Label <span className="text-red-500">*</span>
              </label>
              <select
                name="addressLabel"
                value={formData.addressLabel}
                onChange={handleChange}
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent"
              >
                <option value="Home">Home</option>
                <option value="Office">Office</option>
                <option value="Other">Other</option>
              </select>
            </div>

            {/* Full Address */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Full Address <span className="text-red-500">*</span>
              </label>
              <textarea
                name="fullAddress"
                value={formData.fullAddress}
                onChange={handleChange}
                rows={3}
                className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                  errors.fullAddress ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="House No., Building Name, Street, Area"
              />
              {errors.fullAddress && (
                <p className="mt-1 text-sm text-red-600">{errors.fullAddress}</p>
              )}
            </div>

            {/* Landmark */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Landmark (Optional)
              </label>
              <input
                type="text"
                name="landmark"
                value={formData.landmark}
                onChange={handleChange}
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent"
                placeholder="Nearby landmark for easy location"
              />
            </div>

            {/* City, State, Pincode */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  City <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="city"
                  value={formData.city}
                  onChange={handleChange}
                  className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                    errors.city ? 'border-red-500' : 'border-gray-300'
                  }`}
                  placeholder="City"
                />
                {errors.city && (
                  <p className="mt-1 text-sm text-red-600">{errors.city}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  State <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="state"
                  value={formData.state}
                  onChange={handleChange}
                  className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                    errors.state ? 'border-red-500' : 'border-gray-300'
                  }`}
                  placeholder="State"
                />
                {errors.state && (
                  <p className="mt-1 text-sm text-red-600">{errors.state}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Pincode <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  name="pincode"
                  value={formData.pincode}
                  onChange={handleChange}
                  maxLength={6}
                  className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                    errors.pincode ? 'border-red-500' : 'border-gray-300'
                  }`}
                  placeholder="6-digit pincode"
                />
                {errors.pincode && (
                  <p className="mt-1 text-sm text-red-600">{errors.pincode}</p>
                )}
              </div>
            </div>

            {/* Contact Person */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Contact Person <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                name="contactPerson"
                value={formData.contactPerson}
                onChange={handleChange}
                className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                  errors.contactPerson ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="Full name"
              />
              {errors.contactPerson && (
                <p className="mt-1 text-sm text-red-600">{errors.contactPerson}</p>
              )}
            </div>

            {/* Contact Phone */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Contact Phone <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                name="contactPhone"
                value={formData.contactPhone}
                onChange={handleChange}
                maxLength={10}
                className={`w-full px-4 py-2.5 border rounded-lg focus:ring-2 focus:ring-catering-primary focus:border-transparent ${
                  errors.contactPhone ? 'border-red-500' : 'border-gray-300'
                }`}
                placeholder="10-digit phone number"
              />
              {errors.contactPhone && (
                <p className="mt-1 text-sm text-red-600">{errors.contactPhone}</p>
              )}
            </div>

            {/* Set as Default Checkbox */}
            <div className="flex items-center">
              <input
                type="checkbox"
                name="isDefault"
                checked={formData.isDefault}
                onChange={handleChange}
                className="h-4 w-4 text-catering-primary focus:ring-catering-primary border-gray-300 rounded"
              />
              <label className="ml-2 block text-sm text-gray-700">
                Set as default address
              </label>
            </div>

            {/* Action Buttons */}
            <div className="flex items-center justify-end gap-3 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={onClose}
                disabled={isLoading}
                className="px-6 py-2.5 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2.5 bg-catering-primary text-white font-medium rounded-lg hover:bg-catering-primary/90 transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {isLoading && (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                )}
                {isLoading ? 'Saving...' : (editingAddress ? 'Update Address' : 'Save Address')}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default AddressModal;
