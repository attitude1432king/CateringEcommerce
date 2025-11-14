/*
========================================
File: src/components/common/MediaLightbox.jsx (NEW FILE)
========================================
A reusable fullscreen modal for viewing images and videos.
*/
import React from 'react';
import ReactDOM from 'react-dom'; // Import ReactDOM for portals
import { ownerApiService } from '../../services/ownerApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function MediaLightbox({ mediaItem, onClose }) {
    // FIX: Check for null mediaItem at the very beginning of the component.
    if (!mediaItem) {
        return null;
    }

    // Handle Escape key press to close the modal
    React.useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key === 'Escape') {
                onClose();
            }
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [onClose]);

    const mediaUrl = mediaItem.filePath ? `${API_BASE_URL}${mediaItem.filePath}` : mediaItem.preview;

    const lightboxContent = (
        <div
            className="fixed inset-0 bg-black bg-opacity-80 flex justify-center items-center z-50 p-4"
            onClick={onClose}
        >
            <div
                className="relative max-w-4xl max-h-full"
                onClick={(e) => e.stopPropagation()}
            >
                {ownerApiService.isImageType(mediaItem.mediaType || mediaItem.type) ? (
                    <img
                        src={mediaUrl}
                        alt="Fullscreen media"
                        className="max-w-full max-h-[90vh] object-contain"
                    />
                ) : (
                    <video
                        src={mediaUrl}
                        controls
                        autoPlay
                        className="max-w-full max-h-[90vh]"
                    />
                )}
            </div>
            <button
                onClick={onClose}
                className="absolute top-4 right-4 text-white bg-black bg-opacity-50 rounded-full p-2 hover:bg-opacity-75 transition-colors"
                aria-label="Close fullscreen view"
            >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>
        </div>
    );

    // Use the portal to render the content into the 'portal-root' div in index.html
    return ReactDOM.createPortal(
        lightboxContent,
        document.getElementById('portal-root')
    );
}