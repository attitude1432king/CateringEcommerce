import { useState } from 'react';
import { useAddress } from '../../../contexts/AddressContext';
import AddressModal from './AddressModal';

const SavedAddressesSection = ({ selectedAddressId, onSelectAddress }) => {
  const { addresses, defaultAddress, isLoading, deleteAddress, setAsDefault, canAddAddress } = useAddress();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState(null);

  const handleAddNew = () => {
    if (!canAddAddress()) {
      alert('You can save maximum 5 addresses');
      return;
    }
    setEditingAddress(null);
    setIsModalOpen(true);
  };

  const handleEdit = (address) => {
    setEditingAddress(address);
    setIsModalOpen(true);
  };

  const handleDelete = async (addressId) => {
    if (confirm('Are you sure you want to delete this address?')) {
      try {
        await deleteAddress(addressId);
        if (selectedAddressId === addressId) {
          onSelectAddress(null);
        }
      } catch (error) {
        alert('Failed to delete address. Please try again.');
      }
    }
  };

  const handleSetDefault = async (addressId) => {
    try {
      await setAsDefault(addressId);
    } catch (error) {
      alert('Failed to set default address. Please try again.');
    }
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    setEditingAddress(null);
  };

  if (isLoading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-catering-primary"></div>
        <p className="mt-2 text-neutral-600">Loading addresses...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-800">
          Saved Addresses
        </h3>
        {canAddAddress() && (
          <button
            onClick={handleAddNew}
            className="text-sm font-medium text-catering-primary hover:text-catering-primary/80 flex items-center gap-1"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Add New Address
          </button>
        )}
      </div>

      {/* Address Cards Grid */}
      {addresses.length === 0 ? (
        <div className="bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
          <svg className="w-12 h-12 mx-auto text-gray-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <p className="text-neutral-600 mb-4">No saved addresses yet</p>
          <button
            onClick={handleAddNew}
            className="bg-catering-primary text-white px-6 py-2 rounded-lg hover:bg-catering-primary/90 transition-colors"
          >
            Add Your First Address
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {addresses.map((address) => (
            <div
              key={address.addressId}
              className={`relative border-2 rounded-lg p-4 cursor-pointer transition-all ${
                selectedAddressId === address.addressId
                  ? 'border-catering-primary bg-orange-50 shadow-md'
                  : 'border-gray-200 hover:border-gray-300 hover:shadow-sm'
              }`}
              onClick={() => onSelectAddress(address.addressId)}
            >
              {/* Selection Indicator */}
              {selectedAddressId === address.addressId && (
                <div className="absolute top-3 right-3">
                  <div className="bg-catering-primary rounded-full p-1">
                    <svg className="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                    </svg>
                  </div>
                </div>
              )}

              {/* Address Label & Default Badge */}
              <div className="flex items-center gap-2 mb-2">
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  {address.addressLabel}
                </span>
                {address.isDefault && (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    Default
                  </span>
                )}
              </div>

              {/* Contact Person */}
              <p className="font-medium text-neutral-900 mb-1">{address.contactPerson}</p>

              {/* Full Address */}
              <p className="text-sm text-neutral-600 mb-1">{address.fullAddress}</p>
              {address.landmark && (
                <p className="text-sm text-neutral-500 mb-1">Landmark: {address.landmark}</p>
              )}

              {/* City, State, Pincode */}
              <p className="text-sm text-neutral-600 mb-2">
                {address.city}, {address.state} - {address.pincode}
              </p>

              {/* Phone */}
              <p className="text-sm text-neutral-700 font-medium mb-3">{address.contactPhone}</p>

              {/* Action Buttons */}
              <div className="flex items-center gap-2 pt-3 border-t border-gray-200">
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleEdit(address);
                  }}
                  className="text-xs font-medium text-blue-600 hover:text-blue-700 flex items-center gap-1"
                >
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                  Edit
                </button>

                {!address.isDefault && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      handleSetDefault(address.addressId);
                    }}
                    className="text-xs font-medium text-green-600 hover:text-green-700 flex items-center gap-1"
                  >
                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                    Set Default
                  </button>
                )}

                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleDelete(address.addressId);
                  }}
                  className="text-xs font-medium text-red-600 hover:text-red-700 flex items-center gap-1 ml-auto"
                >
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Address Count Info */}
      {addresses.length > 0 && (
        <p className="text-xs text-neutral-500 text-center">
          {addresses.length} of 5 addresses saved
        </p>
      )}

      {/* Address Modal */}
      {isModalOpen && (
        <AddressModal
          isOpen={isModalOpen}
          onClose={handleModalClose}
          editingAddress={editingAddress}
        />
      )}
    </div>
  );
};

export default SavedAddressesSection;
