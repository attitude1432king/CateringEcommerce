/**
 * TimestampedEvidenceUpload Component
 * Upload photos/videos with GPS + timestamp capture
 */

import { useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { Camera, Video, X, MapPin, Clock } from 'lucide-react';
import { createTimestampedEvidence, formatTimestamp } from '../../../../utils/supervisor/helpers';
import { getUploadUrl, uploadFile } from '../../../../services/api/supervisor';
import toast from 'react-hot-toast';

const TimestampedEvidenceUpload = ({
  label,
  type = 'PHOTO',
  onEvidenceAdded,
  onEvidenceRemoved,
  maxFiles = 10,
  evidenceList = [],
  className = '',
}) => {
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef(null);

  const handleFileChange = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (evidenceList.length >= maxFiles) {
      toast.error(`Maximum ${maxFiles} files allowed`);
      return;
    }

    setUploading(true);

    try {
      // 1. Create timestamped evidence (captures GPS + timestamp)
      const evidence = await createTimestampedEvidence(file, '');

      // 2. Get upload URL
      const urlResponse = await getUploadUrl(file.name, file.type, 'supervisor/evidence');
      if (!urlResponse.success) {
        throw new Error(urlResponse.message);
      }

      // 3. Upload file
      const uploadResponse = await uploadFile(file, urlResponse.data.presignedUrl);
      if (!uploadResponse.success) {
        throw new Error('File upload failed');
      }

      // 4. Create preview
      let localUrl = null;
      if (file.type.startsWith('image/')) {
        localUrl = await new Promise((resolve) => {
          const reader = new FileReader();
          reader.onloadend = () => resolve(reader.result);
          reader.readAsDataURL(file);
        });
      }

      // 5. Complete evidence object
      const completeEvidence = {
        ...evidence,
        url: urlResponse.data.presignedUrl,
        localUrl,
        fileName: file.name,
        fileSize: file.size,
      };

      // 6. Callback
      onEvidenceAdded?.(completeEvidence);
      toast.success('Evidence uploaded successfully');

      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (error) {
      console.error('Evidence upload error:', error);
      toast.error(error.message || 'Failed to upload evidence');
    } finally {
      setUploading(false);
    }
  };

  const handleRemove = (index) => {
    onEvidenceRemoved?.(index);
  };

  const accept = type === 'PHOTO' ? 'image/*' : 'video/*';
  const Icon = type === 'PHOTO' ? Camera : Video;

  return (
    <div className={`w-full ${className}`}>
      {/* Label */}
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-2">
          {label}
        </label>
      )}

      {/* Upload Button */}
      <button
        type="button"
        onClick={() => fileInputRef.current?.click()}
        disabled={uploading || evidenceList.length >= maxFiles}
        className={`
          w-full border-2 border-dashed rounded-lg p-4 text-center transition-colors
          ${uploading ? 'opacity-50 cursor-not-allowed' : 'hover:border-blue-500 cursor-pointer'}
          ${evidenceList.length >= maxFiles ? 'bg-gray-100 cursor-not-allowed' : 'bg-gray-50'}
        `}
      >
        <Icon className="w-8 h-8 mx-auto text-gray-400 mb-2" />
        <p className="text-sm text-gray-600">
          {uploading ? 'Uploading...' : `Capture ${type === 'PHOTO' ? 'Photo' : 'Video'}`}
        </p>
        <p className="text-xs text-gray-500 mt-1">
          GPS + Timestamp will be captured automatically
        </p>
        {evidenceList.length > 0 && (
          <p className="text-xs text-blue-600 mt-1">
            {evidenceList.length} / {maxFiles} files uploaded
          </p>
        )}
      </button>

      {/* Hidden File Input */}
      <input
        ref={fileInputRef}
        type="file"
        accept={accept}
        capture={type === 'PHOTO' ? 'environment' : 'camcorder'}
        onChange={handleFileChange}
        disabled={uploading || evidenceList.length >= maxFiles}
        className="hidden"
      />

      {/* Evidence List */}
      {evidenceList.length > 0 && (
        <div className="mt-4 space-y-3">
          {evidenceList.map((evidence, index) => (
            <div
              key={index}
              className="border border-gray-200 rounded-lg p-3 bg-white"
            >
              <div className="flex items-start gap-3">
                {/* Thumbnail */}
                {evidence.type === 'PHOTO' && evidence.localUrl ? (
                  <img
                    src={evidence.localUrl}
                    alt={`Evidence ${index + 1}`}
                    className="w-16 h-16 object-cover rounded"
                  />
                ) : (
                  <div className="w-16 h-16 bg-gray-100 rounded flex items-center justify-center flex-shrink-0">
                    <Icon className="w-8 h-8 text-gray-400" />
                  </div>
                )}

                {/* Details */}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {evidence.fileName}
                  </p>

                  {/* Timestamp */}
                  <div className="flex items-center gap-1 mt-1 text-xs text-gray-500">
                    <Clock className="w-3 h-3" />
                    {formatTimestamp(evidence.timestamp, 'long')}
                  </div>

                  {/* GPS Location */}
                  {evidence.gpsLocation && (
                    <div className="flex items-center gap-1 mt-1 text-xs text-gray-500">
                      <MapPin className="w-3 h-3" />
                      Location captured
                    </div>
                  )}

                  {/* Description Input */}
                  <input
                    type="text"
                    placeholder="Add description (optional)"
                    value={evidence.description || ''}
                    onChange={(e) => {
                      const updated = [...evidenceList];
                      updated[index].description = e.target.value;
                      onEvidenceAdded?.(updated[index], index);
                    }}
                    className="mt-2 w-full text-xs border border-gray-300 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                {/* Remove Button */}
                <button
                  type="button"
                  onClick={() => handleRemove(index)}
                  className="flex-shrink-0 text-red-500 hover:text-red-700"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

TimestampedEvidenceUpload.propTypes = {
  label: PropTypes.string,
  type: PropTypes.oneOf(['PHOTO', 'VIDEO']),
  onEvidenceAdded: PropTypes.func,
  onEvidenceRemoved: PropTypes.func,
  maxFiles: PropTypes.number,
  evidenceList: PropTypes.arrayOf(PropTypes.object),
  className: PropTypes.string,
};

export default TimestampedEvidenceUpload;
