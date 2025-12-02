/*
========================================
File: src/components/common/SingleFileUploader.jsx (NEW FILE)
========================================
A reusable component for a single file upload slot (e.g., photo, resume).
*/
import React, { useRef } from 'react';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Helper to get the display name and type
const getFileInfo = (fileData) => {
    if (!fileData || fileData.length === 0) {
        return { name: null, type: null, url: null };
    }
    const file = fileData[0];
    if (file.fileObject) { // New file
        return { name: file.fileObject.name, type: file.type, url: file.preview };
    }
    if (file.path) { // Existing file
        return { name: file.path.split('/').pop(), type: file.type, url: `${API_BASE_URL}${file.path}` };
    }
    return { name: null, type: null, url: null };
};

export default function SingleFileUploader({ label, media = [], onMediaChange, error, accept }) {
    const fileInputRef = useRef(null);
    const { name, type, url } = getFileInfo(media);

    const handleFileChange = (files) => {
        if (files && files.length > 0) {
            const file = files[0];
            const newMedia = {
                id: `new_${Date.now()}`,
                type: file.type.startsWith('image/') ? 'image' : 'document',
                preview: URL.createObjectURL(file),
                fileObject: file
            };
            onMediaChange([newMedia]); // Replace media, as this is a single uploader
        }
    };

    const onFileSelect = (e) => handleFileChange(e.target.files);
    const onRemove = (e) => {
        e.preventDefault();
        onMediaChange([]); // Clear the media
    };

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700">{label}</label>
            {name ? (
                <div className="mt-1 flex items-center justify-between p-2 border border-neutral-300 rounded-md">
                    <div className="flex items-center gap-2 overflow-hidden">
                        {type === 'image' ? (
                            <img src={url} alt="Preview" className="w-10 h-10 rounded object-cover" />
                        ) : (
                            <div className="w-10 h-10 rounded bg-neutral-100 flex items-center justify-center">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" /></svg>
                            </div>
                        )}
                        <span className="text-sm text-neutral-700 truncate">{name}</span>
                    </div>
                    <button type="button" onClick={onRemove} className="text-red-600 hover:text-red-800 text-sm font-medium">Remove</button>
                </div>
            ) : (
                <button
                    type="button"
                    onClick={() => fileInputRef.current.click()}
                    className={`mt-1 w-full flex justify-center px-4 py-3 border-2 border-dashed rounded-md ${error ? 'border-red-500' : 'border-neutral-300'}`}
                >
                    <span className="text-sm text-neutral-600">Click to upload a file</span>
                </button>
            )}
            <input
                type="file"
                ref={fileInputRef}
                onChange={onFileSelect}
                className="hidden"
                accept={accept}
            />
            {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
        </div>
    );
}