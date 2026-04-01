/*
========================================
File: src/components/common/MultiImageUploader.jsx (UPDATED)
========================================
A reusable component for uploading multiple images and videos with previews.
*/
import React, { useState, useEffect, useCallback } from 'react';

export default function MultiImageUploader({ onFilesChange, initialFiles = [] }) {
    const [files, setFiles] = useState(initialFiles);

    const handleFileChange = (e) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files).map(file => ({
                file,
                preview: URL.createObjectURL(file),
                type: file.type, // Store the file type
                id: Math.random().toString(36).substr(2, 9)
            }));
            setFiles(prev => [...prev, ...newFiles]);
        }
    };

    const handleRemoveFile = (id) => {
        setFiles(prev => {
            const fileToRemove = prev.find(f => f.id === id);
            if (fileToRemove) {
                // Revoke the object URL to free up memory
                URL.revokeObjectURL(fileToRemove.preview);
            }
            return prev.filter(file => file.id !== id);
        });
    };

    // Notify parent component of changes
    useEffect(() => {
        onFilesChange(files);
        // Cleanup object URLs when component unmounts
        return () => {
            files.forEach(file => URL.revokeObjectURL(file.preview));
        };
    }, [files, onFilesChange]);

    return (
        <div>
            <label htmlFor="multi-image-upload" className="cursor-pointer w-full border-2 border-dashed border-neutral-300 rounded-lg p-6 text-center hover:bg-neutral-50 flex flex-col items-center justify-center">
                <svg className="mx-auto h-12 w-12 text-neutral-400" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true"><path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"></path></svg>
                <span className="mt-2 block text-sm font-medium text-neutral-600">Upload Photos/Videos</span>
                <span className="text-xs text-neutral-500">Drag & drop or click to browse</span>
            </label>
            <input id="multi-image-upload" type="file" multiple accept="image/jpeg, image/png, image/jpg, video/mp4" className="hidden" onChange={handleFileChange} />

            {files.length > 0 && (
                <div className="mt-4 grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-4">
                    {files.map(file => (
                        <div key={file.id} className="relative group aspect-square bg-neutral-100 rounded-md overflow-hidden">
                            {file.type.startsWith('image/') ? (
                                <img src={file.preview} alt="Preview" className="w-full h-full object-cover" />
                            ) : file.type.startsWith('video/') ? (
                                <>
                                    <video src={file.preview} muted playsInline className="w-full h-full object-cover"></video>
                                    <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-20">
                                        <div className="bg-white bg-opacity-70 rounded-full p-2">
                                            <svg className="w-6 h-6 text-neutral-800" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd"></path></svg>
                                        </div>
                                    </div>
                                </>
                            ) : (
                                <div className="flex items-center justify-center h-full text-xs text-neutral-500">Unsupported</div>
                            )}
                            <button onClick={() => handleRemoveFile(file.id)} className="absolute top-1 right-1 bg-black bg-opacity-50 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity z-10">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}