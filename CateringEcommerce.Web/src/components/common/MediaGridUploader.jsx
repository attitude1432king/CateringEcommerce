/*
========================================
File: src/components/common/MediaGridUploader.jsx
========================================
*/
import React, { useState, useEffect, useRef } from 'react';
import { ownerApiService } from '../../services/ownerApi';


const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function MediaGridUploader({ label, subtext, initialMedia = [], onMediaChange, onMediaClick, error, maxFiles = null })
{
    const [media, setMedia] = useState(initialMedia || []);
    const fileInputRef = useRef(null);
    const [isDragging, setIsDragging] = useState(false);

    useEffect(() => {
        setMedia(initialMedia || []);
    }, [initialMedia]);

    const handleFileChange = (files) => {
        if (!files) return;

        const newMediaFiles = Array.from(files).map(file => ({
            id: `new_${Date.now()}_${Math.random()}`,
            type: file.type.startsWith('image/') ? 'image' : 'video',
            preview: URL.createObjectURL(file),
            fileObject: file
        }));

        // When maxFiles=1, replace existing instead of appending
        const updatedMedia = maxFiles === 1
            ? [newMediaFiles[0]]
            : [...media, ...newMediaFiles];
        setMedia(updatedMedia);
        onMediaChange(updatedMedia);
    };

    const onFileSelect = (e) => handleFileChange(e.target.files);
    const onDragOver = (e) => { e.preventDefault(); setIsDragging(true); };
    const onDragLeave = (e) => { e.preventDefault(); setIsDragging(false); };
    const onDrop = (e) => {
        e.preventDefault();
        setIsDragging(false);
        handleFileChange(e.dataTransfer.files);
    };

    const handleRemove = (e, idToRemove) => {
        e.stopPropagation();
        const updatedMedia = media.filter(item => item.id !== idToRemove);
        setMedia(updatedMedia);
        onMediaChange(updatedMedia);
    };

    const mediaUrl = (item) => item.filePath ? `${API_BASE_URL}${item.filePath}` : item.preview;

    return (
        <div>
            <label className="block text-sm font-medium text-neutral-700">{label}</label>
            {subtext && <p className="text-sm text-neutral-500 mb-3 mt-1">{subtext}</p>}
            <div
                onDragOver={onDragOver}
                onDragLeave={onDragLeave}
                onDrop={onDrop}
                className={`p-4 border-2 border-dashed rounded-lg transition-colors ${isDragging ? 'bg-rose-50 border-rose-500' : 'bg-neutral-50 border-neutral-300'}`}
            >
                <div className="grid grid-cols-[repeat(auto-fill,minmax(100px,1fr))] gap-3">
                    {media.map(item => (
                        <div
                            key={item.id}
                            className="relative aspect-square group bg-neutral-100 rounded-lg cursor-pointer"
                            onClick={() => onMediaClick(item)}
                        >
                            {ownerApiService.isImageType(item.mediaType || item.type) ? (
                                <img src={mediaUrl(item)} alt="Media preview" className="w-full h-full object-cover rounded-lg" />
                            ) : (
                                <>
                                    <video src={mediaUrl(item)} className="w-full h-full object-cover rounded-lg" />
                                    <div className="absolute inset-0 flex items-center justify-center">
                                        <div className="bg-black bg-opacity-50 text-white rounded-full p-2">
                                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" viewBox="0 0 20 20" fill="currentColor">
                                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" />
                                            </svg>
                                        </div>
                                    </div>
                                </>
                            )}
                            <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-40 transition-all duration-300 rounded-lg flex items-center justify-center">
                                <div className="text-white opacity-0 group-hover:opacity-100 transition-opacity p-2" title="View Fullscreen">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 1v4m0 0h-4m4 0l-5-5" /></svg>
                                </div>
                                <button
                                    type="button"
                                    title="Remove"
                                    onClick={(e) => handleRemove(e, item.id)}
                                    className="absolute top-1 right-1 bg-black bg-opacity-50 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                </button>
                            </div>
                        </div>
                    ))}
                    {(maxFiles === null || media.length < maxFiles) && (
                        <button type="button" onClick={() => fileInputRef.current.click()} className="flex flex-col items-center justify-center aspect-square border-2 border-dashed rounded-lg text-neutral-400 hover:bg-neutral-100 hover:border-rose-500 hover:text-rose-600 transition-colors">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
                            <span className="text-xs mt-1 font-medium">{maxFiles === 1 ? 'Upload' : 'Add Media'}</span>
                        </button>
                    )}
                </div>
                <input type="file" ref={fileInputRef} onChange={onFileSelect} multiple={maxFiles !== 1} className="hidden" accept="image/png, image/jpeg, video/mp4" />
            </div>
            {error && <p className="text-red-500 text-xs mt-1">{error}</p>}
        </div>
    );
};
