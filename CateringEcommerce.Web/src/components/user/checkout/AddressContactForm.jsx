import React, { useState } from 'react';
import { validateAddressContact } from '../../../utils/checkoutValidator';

const AddressContactForm = ({ formData, onUpdate, onNext, onBack }) => {
  const [errors, setErrors] = useState({});
  const [deliveryAddress, setDeliveryAddress] = useState(formData.deliveryAddress || '');
  const [contactPerson, setContactPerson] = useState(formData.contactPerson || '');
  const [contactPhone, setContactPhone] = useState(formData.contactPhone || '');
  const [contactEmail, setContactEmail] = useState(formData.contactEmail || '');

  const handleNext = () => {
    const dataToValidate = {
      deliveryAddress,
      contactPerson,
      contactPhone,
      contactEmail
    };

    const validation = validateAddressContact(dataToValidate);

    if (!validation.isValid) {
      setErrors(validation.errors);
      return;
    }

    // Clear errors and update parent
    setErrors({});
    onUpdate(dataToValidate);
    onNext();
  };

  return (
    <div className="address-contact-form">
      <h2 className="text-2xl font-bold mb-6">Delivery Address & Contact</h2>

      <div className="space-y-4">
        {/* Delivery Address */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Delivery/Service Address <span className="text-red-500">*</span>
          </label>
          <textarea
            value={deliveryAddress}
            onChange={(e) => setDeliveryAddress(e.target.value)}
            placeholder="Enter complete address with landmarks"
            rows={4}
            maxLength={500}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.deliveryAddress ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.deliveryAddress && (
            <p className="mt-1 text-sm text-red-600">{errors.deliveryAddress}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            {deliveryAddress.length}/500 characters
          </p>
        </div>

        {/* Contact Person */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Contact Person Name <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            value={contactPerson}
            onChange={(e) => setContactPerson(e.target.value)}
            placeholder="Full name of contact person"
            maxLength={100}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.contactPerson ? 'border-red-500' : 'border-gray-300'
            }`}
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
            type="tel"
            value={contactPhone}
            onChange={(e) => setContactPhone(e.target.value)}
            placeholder="10-digit mobile number"
            maxLength={15}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.contactPhone ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.contactPhone && (
            <p className="mt-1 text-sm text-red-600">{errors.contactPhone}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            We'll use this number to coordinate delivery
          </p>
        </div>

        {/* Contact Email */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Contact Email <span className="text-red-500">*</span>
          </label>
          <input
            type="email"
            value={contactEmail}
            onChange={(e) => setContactEmail(e.target.value)}
            placeholder="email@example.com"
            maxLength={100}
            className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent ${
              errors.contactEmail ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.contactEmail && (
            <p className="mt-1 text-sm text-red-600">{errors.contactEmail}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            Order confirmation will be sent to this email
          </p>
        </div>
      </div>

      {/* Navigation Buttons */}
      <div className="flex justify-between mt-8">
        <button
          onClick={onBack}
          className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Back
        </button>
        <button
          onClick={handleNext}
          className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
        >
          Continue
        </button>
      </div>
    </div>
  );
};

export default AddressContactForm;
