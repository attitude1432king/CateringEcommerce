/**
 * CheckInComponent
 * GPS + Photo check-in for event
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { X, Camera, MapPin, Loader } from 'lucide-react';
import { checkIn } from '../../../services/api/supervisor/assignmentApi';
import { getCurrentLocation, formatGPSLocation } from '../../../utils/supervisor/helpers';
import { getUploadUrl, uploadFile } from '../../../services/api/supervisor';
import toast from 'react-hot-toast';

const CheckInComponent = ({ assignmentId, onClose, onSuccess }) => {
  const [photo, setPhoto] = useState(null);
  const [photoPreview, setPhotoPreview] = useState(null);
  const [location, setLocation] = useState(null);
  const [loading, setLoading] = useState(false);

  const handlePhotoCapture = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPhotoPreview(reader.result);
    };
    reader.readAsDataURL(file);
    setPhoto(file);

    // Get location
    try {
      const loc = await getCurrentLocation();
      setLocation(loc);
      toast.success('Location captured');
    } catch (error) {
      toast.error(error.message);
    }
  };

  const handleCheckIn = async () => {
    if (!photo) {
      toast.error('Please capture a photo');
      return;
    }

    if (!location) {
      toast.error('Location is required. Please enable GPS.');
      return;
    }

    setLoading(true);
    try {
      // Upload photo
      const urlResponse = await getUploadUrl(photo.name, photo.type, 'supervisor/checkin');
      if (!urlResponse.success) throw new Error('Failed to get upload URL');

      await uploadFile(photo, urlResponse.data.presignedUrl);

      // Submit check-in
      const supervisorId = localStorage.getItem('supervisorId');
      const response = await checkIn({
        assignmentId,
        supervisorId,
        gpsLocation: formatGPSLocation(location),
        checkInPhoto: urlResponse.data.presignedUrl,
        checkInTime: new Date().toISOString(),
      });

      if (response.success) {
        toast.success('Check-in successful!');
        onSuccess();
      } else {
        toast.error(response.message);
      }
    } catch (error) {
      console.error('Check-in error:', error);
      toast.error('Failed to check in');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-md w-full p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-gray-900">Event Check-In</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="space-y-4">
          {/* Photo Capture */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Capture Selfie <span className="text-red-500">*</span>
            </label>

            {photoPreview ? (
              <div className="relative">
                <img
                  src={photoPreview}
                  alt="Check-in"
                  className="w-full h-48 object-cover rounded-lg"
                />
                <button
                  onClick={() => {
                    setPhoto(null);
                    setPhotoPreview(null);
                  }}
                  className="absolute top-2 right-2 p-1 bg-red-600 text-white rounded-full hover:bg-red-700"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>
            ) : (
              <label className="flex flex-col items-center justify-center w-full h-48 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-blue-500">
                <Camera className="w-12 h-12 text-gray-400 mb-2" />
                <span className="text-sm text-gray-600">Tap to capture photo</span>
                <input
                  type="file"
                  accept="image/*"
                  capture="user"
                  onChange={handlePhotoCapture}
                  className="hidden"
                />
              </label>
            )}
          </div>

          {/* Location Status */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <div className="flex items-start gap-2">
              <MapPin className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
              <div className="flex-1">
                <p className="text-sm font-medium text-blue-900">GPS Location</p>
                {location ? (
                  <p className="text-xs text-blue-700 mt-1">
                    Location captured: {location.latitude.toFixed(6)}, {location.longitude.toFixed(6)}
                  </p>
                ) : (
                  <p className="text-xs text-blue-700 mt-1">
                    Location will be captured with photo
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* Info */}
          <div className="bg-gray-50 rounded-lg p-3">
            <p className="text-xs text-gray-600">
              Your GPS location and photo will be captured for verification. Please ensure you're at the event venue.
            </p>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3 mt-6">
          <button
            onClick={onClose}
            disabled={loading}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={handleCheckIn}
            disabled={loading || !photo || !location}
            className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 flex items-center justify-center gap-2"
          >
            {loading ? (
              <>
                <Loader className="w-4 h-4 animate-spin" />
                Checking In...
              </>
            ) : (
              'Check In'
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

CheckInComponent.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onClose: PropTypes.func.isRequired,
  onSuccess: PropTypes.func.isRequired,
};

export default CheckInComponent;
