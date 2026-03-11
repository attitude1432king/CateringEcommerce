/**
 * Supervisor Portal Helper Functions
 * Common utilities for GPS, timestamps, file validation, etc.
 */

// =====================================================
// GPS & GEOLOCATION
// =====================================================

/**
 * Get current GPS location
 * @returns {Promise<{latitude: number, longitude: number, accuracy: number}>}
 */
export const getCurrentLocation = () => {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      reject(new Error('Geolocation is not supported by this browser'));
      return;
    }

    const timeout = parseInt(import.meta.env.VITE_GPS_TIMEOUT) || 10000;
    const maxAge = parseInt(import.meta.env.VITE_GPS_MAX_AGE) || 60000;

    navigator.geolocation.getCurrentPosition(
      (position) => {
        resolve({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
        });
      },
      (error) => {
        let message = 'Unable to retrieve location';
        switch (error.code) {
          case error.PERMISSION_DENIED:
            message = 'Location permission denied. Please enable location access.';
            break;
          case error.POSITION_UNAVAILABLE:
            message = 'Location information unavailable.';
            break;
          case error.TIMEOUT:
            message = 'Location request timed out.';
            break;
        }
        reject(new Error(message));
      },
      {
        enableHighAccuracy: true,
        timeout,
        maximumAge: maxAge,
      }
    );
  });
};

/**
 * Format GPS location for API
 * @param {Object} location - {latitude, longitude}
 * @returns {string} "latitude,longitude"
 */
export const formatGPSLocation = (location) => {
  return `${location.latitude},${location.longitude}`;
};

/**
 * Parse GPS location string
 * @param {string} locationString - "latitude,longitude"
 * @returns {Object} {latitude, longitude}
 */
export const parseGPSLocation = (locationString) => {
  if (!locationString) return null;
  const [latitude, longitude] = locationString.split(',').map(parseFloat);
  return { latitude, longitude };
};

// =====================================================
// TIMESTAMP UTILITIES
// =====================================================

/**
 * Get current timestamp in ISO format
 * @returns {string}
 */
export const getCurrentTimestamp = () => {
  return new Date().toISOString();
};

/**
 * Format timestamp for display
 * @param {string|Date} timestamp
 * @param {string} format - 'short' | 'long' | 'time'
 * @returns {string}
 */
export const formatTimestamp = (timestamp, format = 'short') => {
  const date = new Date(timestamp);

  switch (format) {
    case 'short':
      return date.toLocaleDateString('en-IN');
    case 'long':
      return date.toLocaleString('en-IN');
    case 'time':
      return date.toLocaleTimeString('en-IN');
    default:
      return date.toLocaleDateString('en-IN');
  }
};

/**
 * Get relative time (e.g., "2 hours ago")
 * @param {string|Date} timestamp
 * @returns {string}
 */
export const getRelativeTime = (timestamp) => {
  const now = new Date();
  const then = new Date(timestamp);
  const diffMs = now - then;
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
  if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
  if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
  return formatTimestamp(timestamp, 'short');
};

// =====================================================
// FILE VALIDATION
// =====================================================

/**
 * Validate file size
 * @param {File} file
 * @returns {boolean}
 */
export const validateFileSize = (file) => {
  const maxSize = parseInt(import.meta.env.VITE_MAX_FILE_SIZE) || 5242880; // 5MB default
  return file.size <= maxSize;
};

/**
 * Validate file type
 * @param {File} file
 * @param {string} category - 'image' | 'video'
 * @returns {boolean}
 */
export const validateFileType = (file, category = 'image') => {
  const allowedTypes = category === 'image'
    ? (import.meta.env.VITE_ALLOWED_IMAGE_TYPES || 'image/jpeg,image/png,image/jpg').split(',')
    : (import.meta.env.VITE_ALLOWED_VIDEO_TYPES || 'video/mp4,video/webm').split(',');

  return allowedTypes.includes(file.type);
};

/**
 * Get file size in readable format
 * @param {number} bytes
 * @returns {string}
 */
export const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
};

/**
 * Validate and prepare file for upload
 * @param {File} file
 * @param {string} category - 'image' | 'video'
 * @returns {Object} {valid: boolean, error?: string, file?: File}
 */
export const prepareFileForUpload = (file, category = 'image') => {
  if (!validateFileSize(file)) {
    return {
      valid: false,
      error: `File size exceeds ${formatFileSize(parseInt(import.meta.env.VITE_MAX_FILE_SIZE))}`,
    };
  }

  if (!validateFileType(file, category)) {
    return {
      valid: false,
      error: `Invalid file type. Allowed: ${category === 'image' ? 'JPEG, PNG' : 'MP4, WebM'}`,
    };
  }

  return { valid: true, file };
};

// =====================================================
// TIMESTAMPED EVIDENCE
// =====================================================

/**
 * Create timestamped evidence object
 * @param {File} file
 * @param {string} description
 * @returns {Promise<Object>}
 */
export const createTimestampedEvidence = async (file, description = '') => {
  try {
    // Get current location
    const location = await getCurrentLocation();

    // Create evidence object
    return {
      type: file.type.startsWith('image/') ? 'PHOTO' : 'VIDEO',
      timestamp: getCurrentTimestamp(),
      gpsLocation: formatGPSLocation(location),
      description,
      file, // File object (to be uploaded separately)
    };
  } catch (error) {
    // If GPS fails, still create evidence without location
    console.warn('GPS unavailable:', error.message);
    return {
      type: file.type.startsWith('image/') ? 'PHOTO' : 'VIDEO',
      timestamp: getCurrentTimestamp(),
      gpsLocation: null,
      description,
      file,
    };
  }
};

// =====================================================
// OTP UTILITIES
// =====================================================

/**
 * Calculate OTP expiry countdown
 * @param {Date|string} expiresAt
 * @returns {number} Seconds remaining
 */
export const getOTPSecondsRemaining = (expiresAt) => {
  const now = new Date();
  const expires = new Date(expiresAt);
  const diffMs = expires - now;
  return Math.max(0, Math.floor(diffMs / 1000));
};

/**
 * Format OTP countdown timer
 * @param {number} seconds
 * @returns {string} "MM:SS"
 */
export const formatOTPCountdown = (seconds) => {
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
};

// =====================================================
// PERMISSION HELPERS
// =====================================================

/**
 * Check if user has permission
 * @param {Object} supervisor - Supervisor object with permission flags
 * @param {string} permission - Permission to check
 * @returns {boolean}
 */
export const hasPermission = (supervisor, permission) => {
  if (!supervisor) return false;

  const permissionMap = {
    'release_payment': supervisor.canReleasePayment,
    'approve_refund': supervisor.canApproveRefund,
    'mentor_others': supervisor.canMentorOthers,
  };

  return permissionMap[permission.toLowerCase()] || false;
};

// =====================================================
// RATING HELPERS
// =====================================================

/**
 * Calculate average rating
 * @param {Array<number>} ratings
 * @returns {number}
 */
export const calculateAverageRating = (ratings) => {
  if (!ratings || ratings.length === 0) return 0;
  const sum = ratings.reduce((acc, rating) => acc + rating, 0);
  return Math.round((sum / ratings.length) * 10) / 10; // Round to 1 decimal
};

/**
 * Get rating display with stars
 * @param {number} rating - 1-5
 * @returns {string}
 */
export const getRatingDisplay = (rating) => {
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;
  const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

  return '⭐'.repeat(fullStars) +
         (hasHalfStar ? '⭐' : '') +
         '☆'.repeat(emptyStars);
};

// =====================================================
// CURRENCY HELPERS
// =====================================================

/**
 * Format currency in Indian Rupees
 * @param {number} amount
 * @returns {string}
 */
export const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(amount);
};

/**
 * Parse currency string to number
 * @param {string} currencyString
 * @returns {number}
 */
export const parseCurrency = (currencyString) => {
  return parseFloat(currencyString.replace(/[^0-9.-]+/g, ''));
};

// =====================================================
// CLIENT IP ADDRESS
// =====================================================

/**
 * Get client IP address (for OTP verification)
 * @returns {Promise<string>}
 */
export const getClientIPAddress = async () => {
  try {
    const response = await fetch('https://api.ipify.org?format=json');
    const data = await response.json();
    return data.ip;
  } catch (error) {
    console.error('Failed to get IP address:', error);
    return 'unknown';
  }
};

// =====================================================
// LOCAL STORAGE HELPERS
// =====================================================

/**
 * Save supervisor data to localStorage
 * @param {Object} supervisor
 */
export const saveSupervisorToLocalStorage = (supervisor) => {
  localStorage.setItem('supervisorId', supervisor.supervisorId);
  localStorage.setItem('supervisorData', JSON.stringify(supervisor));
};

/**
 * Get supervisor data from localStorage
 * @returns {Object|null}
 */
export const getSupervisorFromLocalStorage = () => {
  const data = localStorage.getItem('supervisorData');
  return data ? JSON.parse(data) : null;
};

/**
 * Clear supervisor data from localStorage
 */
export const clearSupervisorFromLocalStorage = () => {
  localStorage.removeItem('supervisorId');
  localStorage.removeItem('supervisorData');
  localStorage.removeItem('supervisorToken');
};
