import { useState } from 'react';
import { X, Play, Pause, ChevronLeft, ChevronRight } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

/**
 * Shared Media Viewer Component
 * Handles images, videos (.mp4, .webm, .ogg, .mov, .avi), and documents (.pdf, .doc, .docx)
 *
 * Props:
 *   mediaItems   - Array of { filePath, fileName, label }
 *   currentIndex - Active item index
 *   onClose      - Called when viewer is closed
 *   onNavigate   - Called with new index when user navigates prev/next
 */
const MediaViewer = ({ mediaItems, currentIndex, onClose, onNavigate }) => {
    const [isPlaying, setIsPlaying] = useState(true);
    const currentMedia = mediaItems[currentIndex];

    if (!currentMedia) return null;

    const mediaUrl = currentMedia.filePath.startsWith('http')
        ? currentMedia.filePath
        : `${API_BASE_URL}${currentMedia.filePath}`;

    const isVideo = (item) => {
        const videoExtensions = ['.mp4', '.webm', '.ogg', '.mov', '.avi'];
        return videoExtensions.some(ext => item.filePath.toLowerCase().endsWith(ext));
    };

    const isDocument = (item) => {
        const docExtensions = ['.pdf', '.doc', '.docx'];
        return docExtensions.some(ext => item.filePath.toLowerCase().endsWith(ext));
    };

    const handleVideoToggle = () => {
        const video = document.getElementById('media-video');
        if (video) {
            if (isPlaying) {
                video.pause();
            } else {
                video.play();
            }
            setIsPlaying(!isPlaying);
        }
    };

    const handlePrevious = () => {
        if (currentIndex > 0) {
            onNavigate(currentIndex - 1);
            setIsPlaying(true);
        }
    };

    const handleNext = () => {
        if (currentIndex < mediaItems.length - 1) {
            onNavigate(currentIndex + 1);
            setIsPlaying(true);
        }
    };

    const renderMedia = () => {
        if (isVideo(currentMedia)) {
            return (
                <div className="relative w-full h-full flex items-center justify-center">
                    <video
                        id="media-video"
                        src={mediaUrl}
                        autoPlay
                        controls
                        className="max-w-full max-h-full"
                        onPlay={() => setIsPlaying(true)}
                        onPause={() => setIsPlaying(false)}
                    >
                        Your browser does not support the video tag.
                    </video>
                    <button
                        onClick={handleVideoToggle}
                        className="absolute bottom-4 left-1/2 transform -translate-x-1/2 p-3 bg-black bg-opacity-50 text-white rounded-full hover:bg-opacity-70 transition-all"
                    >
                        {isPlaying ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6" />}
                    </button>
                </div>
            );
        } else if (isDocument(currentMedia)) {
            return (
                <div className="w-full h-full flex items-center justify-center">
                    <iframe
                        src={mediaUrl}
                        className="w-full h-full border-0"
                        title={currentMedia.fileName || 'Document'}
                    />
                </div>
            );
        } else {
            return (
                <div className="w-full h-full flex items-center justify-center p-4">
                    <img
                        src={mediaUrl}
                        alt={currentMedia.fileName || 'Media'}
                        className="max-w-full max-h-full object-contain"
                    />
                </div>
            );
        }
    };

    return (
        <div className="fixed inset-0 z-[60] flex items-center justify-center">
            {/* Backdrop */}
            <div
                className="absolute inset-0 bg-black bg-opacity-90"
                onClick={onClose}
            ></div>

            {/* Media Container */}
            <div className="relative w-full h-full max-w-7xl max-h-screen p-4 flex flex-col">
                {/* Header */}
                <div className="flex items-center justify-between mb-4 z-10">
                    <div className="text-white">
                        <p className="text-lg font-medium">
                            {currentMedia.fileName || currentMedia.label || 'Media'}
                        </p>
                        <p className="text-sm text-gray-300">
                            {currentIndex + 1} of {mediaItems.length}
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        className="p-2 text-white hover:bg-white hover:bg-opacity-20 rounded-lg transition-colors"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Media Content */}
                <div className="flex-1 relative overflow-hidden rounded-lg">
                    {renderMedia()}
                </div>

                {/* Navigation */}
                {mediaItems.length > 1 && (
                    <div className="flex items-center justify-center mt-4 space-x-4">
                        <button
                            onClick={handlePrevious}
                            disabled={currentIndex === 0}
                            className="p-2 bg-white bg-opacity-20 text-white rounded-lg hover:bg-opacity-30 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                        >
                            <ChevronLeft className="w-6 h-6" />
                        </button>
                        <span className="text-white text-sm">
                            {currentIndex + 1} / {mediaItems.length}
                        </span>
                        <button
                            onClick={handleNext}
                            disabled={currentIndex === mediaItems.length - 1}
                            className="p-2 bg-white bg-opacity-20 text-white rounded-lg hover:bg-opacity-30 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                        >
                            <ChevronRight className="w-6 h-6" />
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};

export default MediaViewer;
