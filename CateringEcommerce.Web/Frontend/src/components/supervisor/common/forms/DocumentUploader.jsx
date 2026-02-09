/**
 * DocumentUploader Component
 * File upload with preview and validation
 */

import { useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { Upload, X, FileText, Image, Film } from 'lucide-react';
import { prepareFileForUpload, formatFileSize } from '../../../../utils/supervisor/helpers';
import { getUploadUrl, uploadFile } from '../../../../services/api/supervisor';
import toast from 'react-hot-toast';

const DocumentUploader = ({
  label,
  accept = 'image/*',
  category = 'image',
  maxSize,
  onUploadComplete,
  onRemove,
  value,
  error,
  helperText,
  required = false,
  className = '',
}) => {
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState(value || null);
  const fileInputRef = useRef(null);

  const getFileIcon = (fileType) => {
    if (fileType?.startsWith('image/')) return Image;
    if (fileType?.startsWith('video/')) return Film;
    return FileText;
  };

  const handleFileChange = async (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file
    const validation = prepareFileForUpload(file, category);
    if (!validation.valid) {
      toast.error(validation.error);
      return;
    }

    setUploading(true);

    try {
      // 1. Get upload URL
      const urlResponse = await getUploadUrl(file.name, file.type, 'supervisor');
      if (!urlResponse.success) {
        throw new Error(urlResponse.message);
      }

      // 2. Upload file
      const uploadResponse = await uploadFile(file, urlResponse.data.presignedUrl);
      if (!uploadResponse.success) {
        throw new Error('File upload failed');
      }

      // 3. Set preview
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onloadend = () => {
          setPreview({
            type: 'image',
            url: urlResponse.data.presignedUrl,
            localUrl: reader.result,
            name: file.name,
            size: file.size,
          });
        };
        reader.readAsDataURL(file);
      } else {
        setPreview({
          type: file.type.startsWith('video/') ? 'video' : 'document',
          url: urlResponse.data.presignedUrl,
          name: file.name,
          size: file.size,
        });
      }

      // 4. Callback with URL
      onUploadComplete?.(urlResponse.data.presignedUrl, file);
      toast.success('File uploaded successfully');
    } catch (error) {
      console.error('Upload error:', error);
      toast.error(error.message || 'Failed to upload file');
    } finally {
      setUploading(false);
    }
  };

  const handleRemove = () => {
    setPreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
    onRemove?.();
  };

  const FileIcon = preview ? getFileIcon(preview.type) : Upload;

  return (
    <div className={`w-full ${className}`}>
      {/* Label */}
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-2">
          {label}
          {required && <span className="text-red-500 ml-1">*</span>}
        </label>
      )}

      {/* Upload Area */}
      <div className="relative">
        {!preview ? (
          <div
            onClick={() => fileInputRef.current?.click()}
            className={`
              border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors
              ${error ? 'border-red-300 bg-red-50' : 'border-gray-300 hover:border-blue-500 bg-gray-50'}
              ${uploading ? 'opacity-50 cursor-not-allowed' : ''}
            `}
          >
            <Upload className="w-12 h-12 mx-auto text-gray-400 mb-3" />
            <p className="text-sm text-gray-600 mb-1">
              {uploading ? 'Uploading...' : 'Click to upload or drag and drop'}
            </p>
            <p className="text-xs text-gray-500">
              {category === 'image' ? 'PNG, JPG up to' : 'MP4, WebM up to'} {formatFileSize(maxSize || parseInt(import.meta.env.VITE_MAX_FILE_SIZE))}
            </p>
          </div>
        ) : (
          <div className="border-2 border-gray-300 rounded-lg p-4 bg-white">
            <div className="flex items-start gap-4">
              {/* Preview */}
              <div className="flex-shrink-0">
                {preview.type === 'image' && preview.localUrl ? (
                  <img
                    src={preview.localUrl}
                    alt="Preview"
                    className="w-16 h-16 object-cover rounded"
                  />
                ) : (
                  <div className="w-16 h-16 bg-gray-100 rounded flex items-center justify-center">
                    <FileIcon className="w-8 h-8 text-gray-400" />
                  </div>
                )}
              </div>

              {/* File Info */}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate">
                  {preview.name}
                </p>
                <p className="text-xs text-gray-500">
                  {formatFileSize(preview.size)}
                </p>
              </div>

              {/* Remove Button */}
              <button
                type="button"
                onClick={handleRemove}
                className="flex-shrink-0 text-red-500 hover:text-red-700"
                disabled={uploading}
              >
                <X className="w-5 h-5" />
              </button>
            </div>
          </div>
        )}

        {/* Hidden File Input */}
        <input
          ref={fileInputRef}
          type="file"
          accept={accept}
          onChange={handleFileChange}
          disabled={uploading}
          className="hidden"
        />
      </div>

      {/* Helper Text */}
      {helperText && !error && (
        <p className="mt-1 text-xs text-gray-500">{helperText}</p>
      )}

      {/* Error Message */}
      {error && (
        <p className="mt-1 text-xs text-red-600">{error}</p>
      )}
    </div>
  );
};

DocumentUploader.propTypes = {
  label: PropTypes.string,
  accept: PropTypes.string,
  category: PropTypes.oneOf(['image', 'video', 'document']),
  maxSize: PropTypes.number,
  onUploadComplete: PropTypes.func,
  onRemove: PropTypes.func,
  value: PropTypes.object,
  error: PropTypes.string,
  helperText: PropTypes.string,
  required: PropTypes.bool,
  className: PropTypes.string,
};

export default DocumentUploader;
