using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.BAL.Base.User
{
    public class UserAddressService
    {
        private readonly string _connectionString;
        private readonly UserAddressRepository _addressRepository;
        private const int MAX_ADDRESSES_PER_USER = 5;

        public UserAddressService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _addressRepository = new UserAddressRepository(connectionString);
        }

        // ===================================
        // GET USER ADDRESSES
        // ===================================
        public async Task<List<SavedAddressDto>> GetUserAddressesAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                return await _addressRepository.GetUserAddressesAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user addresses: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ADDRESS BY ID
        // ===================================
        public async Task<SavedAddressDto> GetAddressByIdAsync(long addressId, long userId)
        {
            try
            {
                if (addressId <= 0)
                {
                    throw new ArgumentException("Invalid address ID", nameof(addressId));
                }

                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                var address = await _addressRepository.GetAddressByIdAsync(addressId, userId);
                if (address == null)
                {
                    throw new InvalidOperationException("Address not found or does not belong to the user.");
                }

                return address;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving address: " + ex.Message, ex);
            }
        }

        // ===================================
        // CREATE ADDRESS
        // ===================================
        public async Task<SavedAddressDto> CreateAddressAsync(long userId, CreateAddressDto addressData)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                // Validate max addresses per user (5)
                int currentCount = await _addressRepository.CountUserAddressesAsync(userId);
                if (currentCount >= MAX_ADDRESSES_PER_USER)
                {
                    throw new InvalidOperationException($"You can only have a maximum of {MAX_ADDRESSES_PER_USER} saved addresses. Please delete an existing address to add a new one.");
                }

                // Validate address data
                ValidateAddressData(addressData.AddressLabel, addressData.FullAddress, addressData.City,
                    addressData.State, addressData.Pincode, addressData.ContactPerson, addressData.ContactPhone);

                // If this is the first address, make it default
                if (currentCount == 0)
                {
                    addressData.IsDefault = true;
                }

                // If setting as default, unset other defaults
                if (addressData.IsDefault)
                {
                    // This will be handled in the repository when setting default
                    // But we need to call it here to ensure consistency
                    // Actually, we'll handle it in SetDefaultAddressAsync instead
                }

                // Insert address
                long addressId = await _addressRepository.InsertAddressAsync(userId, addressData);
                if (addressId <= 0)
                {
                    throw new InvalidOperationException("Failed to create address. Please try again.");
                }

                // If marked as default, ensure it's set correctly
                if (addressData.IsDefault)
                {
                    await _addressRepository.SetDefaultAddressAsync(addressId, userId);
                }

                // Retrieve and return the created address
                var createdAddress = await _addressRepository.GetAddressByIdAsync(addressId, userId);
                if (createdAddress == null)
                {
                    throw new InvalidOperationException("Address created but failed to retrieve details.");
                }

                return createdAddress;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating address: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE ADDRESS
        // ===================================
        public async Task<SavedAddressDto> UpdateAddressAsync(long userId, UpdateAddressDto addressData)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                if (addressData.AddressId <= 0)
                {
                    throw new ArgumentException("Invalid address ID", nameof(addressData.AddressId));
                }

                // Validate the address exists and belongs to the user
                var existingAddress = await _addressRepository.GetAddressByIdAsync(addressData.AddressId, userId);
                if (existingAddress == null)
                {
                    throw new InvalidOperationException("Address not found or does not belong to the user.");
                }

                // Validate address data
                ValidateAddressData(addressData.AddressLabel, addressData.FullAddress, addressData.City,
                    addressData.State, addressData.Pincode, addressData.ContactPerson, addressData.ContactPhone);

                // If setting as default, unset other defaults
                if (addressData.IsDefault && !existingAddress.IsDefault)
                {
                    await _addressRepository.SetDefaultAddressAsync(addressData.AddressId, userId);
                }

                // Update address
                bool updated = await _addressRepository.UpdateAddressAsync(userId, addressData);
                if (!updated)
                {
                    throw new InvalidOperationException("Failed to update address. Please try again.");
                }

                // Retrieve and return the updated address
                var updatedAddress = await _addressRepository.GetAddressByIdAsync(addressData.AddressId, userId);
                if (updatedAddress == null)
                {
                    throw new InvalidOperationException("Address updated but failed to retrieve details.");
                }

                return updatedAddress;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating address: " + ex.Message, ex);
            }
        }

        // ===================================
        // DELETE ADDRESS
        // ===================================
        public async Task<bool> DeleteAddressAsync(long addressId, long userId)
        {
            try
            {
                if (addressId <= 0)
                {
                    throw new ArgumentException("Invalid address ID", nameof(addressId));
                }

                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                // Validate the address exists and belongs to the user
                var existingAddress = await _addressRepository.GetAddressByIdAsync(addressId, userId);
                if (existingAddress == null)
                {
                    throw new InvalidOperationException("Address not found or does not belong to the user.");
                }

                // Delete address (soft delete)
                bool deleted = await _addressRepository.DeleteAddressAsync(addressId, userId);
                if (!deleted)
                {
                    throw new InvalidOperationException("Failed to delete address. Please try again.");
                }

                // If the deleted address was default, set another address as default
                if (existingAddress.IsDefault)
                {
                    var remainingAddresses = await _addressRepository.GetUserAddressesAsync(userId);
                    if (remainingAddresses.Count > 0)
                    {
                        // Set the first address as default
                        await _addressRepository.SetDefaultAddressAsync(remainingAddresses[0].AddressId, userId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting address: " + ex.Message, ex);
            }
        }

        // ===================================
        // SET DEFAULT ADDRESS
        // ===================================
        public async Task<bool> SetDefaultAddressAsync(long addressId, long userId)
        {
            try
            {
                if (addressId <= 0)
                {
                    throw new ArgumentException("Invalid address ID", nameof(addressId));
                }

                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID", nameof(userId));
                }

                // Validate the address exists and belongs to the user
                var existingAddress = await _addressRepository.GetAddressByIdAsync(addressId, userId);
                if (existingAddress == null)
                {
                    throw new InvalidOperationException("Address not found or does not belong to the user.");
                }

                // Set as default
                bool updated = await _addressRepository.SetDefaultAddressAsync(addressId, userId);
                if (!updated)
                {
                    throw new InvalidOperationException("Failed to set default address. Please try again.");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error setting default address: " + ex.Message, ex);
            }
        }

        // ===================================
        // VALIDATE ADDRESS DATA
        // ===================================
        private void ValidateAddressData(string addressLabel, string fullAddress, string city,
            string state, string pincode, string contactPerson, string contactPhone)
        {
            // Validate address label
            if (string.IsNullOrWhiteSpace(addressLabel))
            {
                throw new ArgumentException("Address label is required.", nameof(addressLabel));
            }

            var validLabels = new[] { "Home", "Office", "Other" };
            if (!Array.Exists(validLabels, label => label.Equals(addressLabel, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Address label must be 'Home', 'Office', or 'Other'.", nameof(addressLabel));
            }

            // Validate full address
            if (string.IsNullOrWhiteSpace(fullAddress) || fullAddress.Length > 500)
            {
                throw new ArgumentException("Full address is required and must not exceed 500 characters.", nameof(fullAddress));
            }

            // Validate city
            if (string.IsNullOrWhiteSpace(city) || city.Length > 100)
            {
                throw new ArgumentException("City is required and must not exceed 100 characters.", nameof(city));
            }

            // Validate state
            if (string.IsNullOrWhiteSpace(state) || state.Length > 100)
            {
                throw new ArgumentException("State is required and must not exceed 100 characters.", nameof(state));
            }

            // Validate pincode (6 digits)
            if (string.IsNullOrWhiteSpace(pincode) || !System.Text.RegularExpressions.Regex.IsMatch(pincode, @"^\d{6}$"))
            {
                throw new ArgumentException("Pincode must be exactly 6 digits.", nameof(pincode));
            }

            // Validate contact person
            if (string.IsNullOrWhiteSpace(contactPerson) || contactPerson.Length > 100)
            {
                throw new ArgumentException("Contact person is required and must not exceed 100 characters.", nameof(contactPerson));
            }

            // Validate contact phone (10-digit Indian number starting with 6-9)
            if (string.IsNullOrWhiteSpace(contactPhone) || !System.Text.RegularExpressions.Regex.IsMatch(contactPhone, @"^[6-9]\d{9}$"))
            {
                throw new ArgumentException("Contact phone must be a valid 10-digit Indian mobile number.", nameof(contactPhone));
            }
        }
    }
}
