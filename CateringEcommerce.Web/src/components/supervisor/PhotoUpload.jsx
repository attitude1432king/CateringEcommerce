/**
 * PhotoUpload Component
 * GPS-enabled photo capture for event supervision evidence
 * Validates: minimum 3 photos per phase, GPS mandatory, timestamp auto-captured
 */

import { useState, useRef } from 'react';
import PropTypes from 'prop-types';
import {
    Camera, MapPin, Clock, Trash2, Upload, CheckCircle, AlertTriangle, Image, X,
} from 'lucide-react';
import { uploadTimestampedEvidence } from '../../services/api/supervisor/eventSupervisionApi';
import toast from 'react-hot-toast';

const PHASE_LABELS = {
    PRE_EVENT: 'Pre-Event',
    DURING_EVENT: 'During Event',
    POST_EVENT: 'Post-Event',
};

const MINIMUM_PHOTOS = 3;

const PhotoUpload = ({ assignmentId, phase, onUploadComplete }) => {
    const [photos, setPhotos] = useState([]);
    const [uploading, setUploading] = useState(false);
    const [gpsStatus, setGpsStatus] = useState('idle'); // idle | loading | success | error
    const fileInputRef = useRef(null);

    /** Get current GPS coordinates */
    const getGPSLocation = () => {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error('Geolocation not supported by browser'));
                return;
            }
            setGpsStatus('loading');
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    setGpsStatus('success');
                    resolve(`${position.coords.latitude},${position.coords.longitude}`);
                },
                (error) => {
                    setGpsStatus('error');
                    reject(new Error(`GPS error: ${error.message}`));
                },
                { enableHighAccuracy: true, timeout: 10000 }
            );
        });
    };

    /** Handle file selection from camera/gallery */
    const handleFileSelect = async (e) => {
        const files = Array.from(e.target.files);
        if (!files.length) return;

        let gpsLocation;
        try {
            gpsLocation = await getGPSLocation();
        } catch {
            toast.error('GPS location is required. Please enable location access.');
            return;
        }

        const newPhotos = [];
        for (const file of files) {
            if (!file.type.startsWith('image/')) {
                toast.error(`${file.name} is not an image file`);
                continue;
            }

            const base64 = await fileToBase64(file);
            newPhotos.push({
                Type: 'PHOTO',
                Url: base64,
                Timestamp: new Date().toISOString(),
                GPSLocation: gpsLocation,
                Description: '',
                _preview: URL.createObjectURL(file),
                _name: file.name,
            });
        }

        setPhotos((prev) => [...prev, ...newPhotos]);

        // Reset input
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    /** Convert file to base64 */
    const fileToBase64 = (file) => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    };

    /** Update photo description */
    const updateDescription = (index, description) => {
        setPhotos((prev) =>
            prev.map((p, i) => (i === index ? { ...p, Description: description } : p))
        );
    };

    /** Remove a photo */
    const removePhoto = (index) => {
        setPhotos((prev) => {
            const removed = prev[index];
            if (removed._preview) URL.revokeObjectURL(removed._preview);
            return prev.filter((_, i) => i !== index);
        });
    };

    /** Submit photos to backend */
    const submitPhotos = async () => {
        if (photos.length < MINIMUM_PHOTOS) {
            toast.error(`Minimum ${MINIMUM_PHOTOS} photos required for ${PHASE_LABELS[phase]}. Current: ${photos.length}`);
            return;
        }

        setUploading(true);
        try {
            const evidence = photos.map(({ Type, Url, Timestamp, GPSLocation, Description }) => ({
                Type,
                Url,
                Timestamp,
                GPSLocation,
                Description,
            }));

            const result = await uploadTimestampedEvidence(assignmentId, evidence, phase);

            if (result.success) {
                toast.success(`${photos.length} photos uploaded successfully!`);
                // Cleanup previews
                photos.forEach((p) => {
                    if (p._preview) URL.revokeObjectURL(p._preview);
                });
                setPhotos([]);
                onUploadComplete?.();
            } else {
                toast.error(result.message || 'Upload failed');
            }
        } catch {
            toast.error('Failed to upload photos. Please try again.');
        } finally {
            setUploading(false);
        }
    };

    const phaseLabel = PHASE_LABELS[phase] || phase;

    return (
        <div className="space-y-5">
            {/* Header */}
            <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center">
                    <Camera className="w-5 h-5 text-amber-600" />
                </div>
                <div>
                    <h3 className="text-lg font-bold text-neutral-800">Upload Photos ({phaseLabel})</h3>
                    <p className="text-sm text-neutral-500">
                        Minimum {MINIMUM_PHOTOS} photos required. GPS location is captured automatically.
                    </p>
                </div>
            </div>

            {/* GPS Status */}
            <div className={`flex items-center gap-2 px-4 py-2.5 rounded-xl border-2 text-sm ${
                gpsStatus === 'success' ? 'bg-green-50 border-green-200 text-green-700' :
                gpsStatus === 'error' ? 'bg-red-50 border-red-200 text-red-700' :
                gpsStatus === 'loading' ? 'bg-blue-50 border-blue-200 text-blue-700' :
                'bg-neutral-50 border-neutral-200 text-neutral-600'
            }`}>
                <MapPin className="w-4 h-4" />
                {gpsStatus === 'success' && 'GPS location captured'}
                {gpsStatus === 'error' && 'GPS not available - please enable location access'}
                {gpsStatus === 'loading' && 'Acquiring GPS location...'}
                {gpsStatus === 'idle' && 'GPS location will be captured when you add photos'}
            </div>

            {/* Capture Button */}
            <div>
                <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    capture="environment"
                    multiple
                    onChange={handleFileSelect}
                    className="hidden"
                />
                <button
                    type="button"
                    onClick={() => fileInputRef.current?.click()}
                    disabled={uploading}
                    className="w-full flex items-center justify-center gap-3 px-6 py-4 border-2 border-dashed border-rose-300 rounded-xl text-rose-600 font-semibold hover:bg-rose-50 hover:border-rose-400 transition-all duration-200 disabled:opacity-50"
                >
                    <Camera className="w-5 h-5" />
                    Capture / Select Photos
                </button>
            </div>

            {/* Photo Count */}
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-2 text-sm">
                    <Image className="w-4 h-4 text-neutral-500" />
                    <span className="text-neutral-600">
                        <span className="font-semibold">{photos.length}</span> / {MINIMUM_PHOTOS} minimum photos
                    </span>
                </div>
                {photos.length >= MINIMUM_PHOTOS && (
                    <span className="flex items-center gap-1.5 text-sm text-green-600 font-semibold">
                        <CheckCircle className="w-4 h-4" /> Minimum met
                    </span>
                )}
                {photos.length > 0 && photos.length < MINIMUM_PHOTOS && (
                    <span className="flex items-center gap-1.5 text-sm text-amber-600 font-semibold">
                        <AlertTriangle className="w-4 h-4" /> Need {MINIMUM_PHOTOS - photos.length} more
                    </span>
                )}
            </div>

            {/* Photo Grid */}
            {photos.length > 0 && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {photos.map((photo, index) => (
                        <div key={index} className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm overflow-hidden">
                            <div className="relative">
                                <img
                                    src={photo._preview}
                                    alt={`Photo ${index + 1}`}
                                    className="w-full h-40 object-cover"
                                />
                                <button
                                    type="button"
                                    onClick={() => removePhoto(index)}
                                    className="absolute top-2 right-2 p-1.5 bg-white/90 rounded-lg text-red-500 hover:bg-red-50 hover:text-red-600 transition-colors shadow-sm"
                                >
                                    <X className="w-4 h-4" />
                                </button>
                                <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent px-3 py-2">
                                    <span className="text-white text-xs font-medium">Photo {index + 1}</span>
                                </div>
                            </div>
                            <div className="p-3 space-y-2">
                                <div className="flex items-center gap-2 text-xs text-neutral-500">
                                    <MapPin className="w-3 h-3" />
                                    <span className="truncate">{photo.GPSLocation}</span>
                                </div>
                                <div className="flex items-center gap-2 text-xs text-neutral-500">
                                    <Clock className="w-3 h-3" />
                                    <span>{new Date(photo.Timestamp).toLocaleString()}</span>
                                </div>
                                <input
                                    type="text"
                                    value={photo.Description}
                                    onChange={(e) => updateDescription(index, e.target.value)}
                                    placeholder="Add description..."
                                    className="w-full px-3 py-2 text-sm border-2 border-neutral-200 rounded-lg focus:border-rose-400 focus:ring-2 focus:ring-rose-100 focus:outline-none transition-all"
                                />
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {/* Empty State */}
            {photos.length === 0 && (
                <div className="text-center py-8 bg-neutral-50 rounded-xl border-2 border-neutral-100">
                    <Camera className="w-12 h-12 text-neutral-300 mx-auto mb-3" />
                    <p className="text-sm text-neutral-500">No photos captured yet</p>
                    <p className="text-xs text-neutral-400 mt-1">Tap the button above to start capturing</p>
                </div>
            )}

            {/* Submit Button */}
            <button
                type="button"
                onClick={submitPhotos}
                disabled={uploading || photos.length < MINIMUM_PHOTOS}
                className={`w-full flex items-center justify-center gap-2 px-6 py-3.5 rounded-xl font-semibold shadow-lg transition-all duration-200 ${
                    photos.length >= MINIMUM_PHOTOS && !uploading
                        ? 'bg-gradient-to-r from-rose-600 to-rose-500 text-white hover:shadow-xl'
                        : 'bg-neutral-200 text-neutral-400 cursor-not-allowed'
                }`}
            >
                {uploading ? (
                    <>
                        <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                        Uploading...
                    </>
                ) : (
                    <>
                        <Upload className="w-5 h-5" />
                        Submit {photos.length} Photo{photos.length !== 1 ? 's' : ''}
                    </>
                )}
            </button>

            {/* Info Box */}
            <div className="bg-amber-50 border-l-4 border-amber-400 rounded-lg p-4">
                <p className="text-sm text-amber-800">
                    All photos must include GPS location and will be timestamped automatically.
                    Ensure good lighting and clear visibility for verification.
                </p>
            </div>
        </div>
    );
};

PhotoUpload.propTypes = {
    assignmentId: PropTypes.number.isRequired,
    phase: PropTypes.oneOf(['PRE_EVENT', 'DURING_EVENT', 'POST_EVENT']).isRequired,
    onUploadComplete: PropTypes.func,
};

export default PhotoUpload;
