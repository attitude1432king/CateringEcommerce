/*
========================================
File: src/components/common/FileUploader.jsx (NEW FILE)
========================================
A reusable component for uploading a single Image (with crop) or PDF.
*/
import React, { useState, useRef } from 'react';
import ReactCrop, { centerCrop, makeAspectCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

// Utility to center the crop aspect ratio
function centerAspectCrop(mediaWidth, mediaHeight, aspect) {
    return centerCrop(
        makeAspectCrop({ unit: '%', width: 90 }, aspect, mediaWidth, mediaHeight),
        mediaWidth,
        mediaHeight
    );
}

export default function FileUploader({ onFileCropped, aspect = 16 / 9, acceptedTypes = "image/jpeg, image/png, application/pdf" }) {
    const [sourceFile, setSourceFile] = useState(null); // Holds the original file data for cropping
    const [crop, setCrop] = useState();
    const [completedCrop, setCompletedCrop] = useState();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const imgRef = useRef(null);
    const previewCanvasRef = useRef(null);
    const fileInputRef = useRef(null);

    function onSelectFile(e) {
        if (e.target.files && e.target.files.length > 0) {
            const file = e.target.files[0];
            const reader = new FileReader();
            reader.addEventListener('load', () => {
                const result = reader.result?.toString() || '';
                if (file.type.startsWith('image/')) {
                    setSourceFile(result);
                    setIsModalOpen(true);
                } else {
                    // For non-image files like PDF, just pass the Base64 data up
                    onFileCropped({
                        base64: result,
                        name: file.name,
                        type: file.type
                    });
                }
            });
            reader.readAsDataURL(file);
        }
    }

    function onImageLoad(e) {
        const { width, height } = e.currentTarget;
        setCrop(centerAspectCrop(width, height, aspect));
    }

    async function handleCrop() {
        if (!previewCanvasRef.current || !imgRef.current || !completedCrop) return;
        const image = imgRef.current;
        const canvas = previewCanvasRef.current;
        const scaleX = image.naturalWidth / image.width;
        const scaleY = image.naturalHeight / image.height;
        canvas.width = completedCrop.width * scaleX;
        canvas.height = completedCrop.height * scaleY;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(image, completedCrop.x * scaleX, completedCrop.y * scaleY, completedCrop.width * scaleX, completedCrop.height * scaleY, 0, 0, canvas.width, canvas.height);

        const base64Image = canvas.toDataURL('image/jpeg');
        onFileCropped({
            base64: base64Image,
            name: "cropped_image.jpg",
            type: 'image/jpeg'
        });
        setIsModalOpen(false);
        setSourceFile('');
    }

    return (
        <>
            <input type="file" accept={acceptedTypes} onChange={onSelectFile} className="hidden" ref={fileInputRef} />
            <button type="button" onClick={() => fileInputRef.current.click()} className="w-full h-full text-sm text-rose-600 font-semibold hover:underline">
                Click to upload
            </button>

            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
                    <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-lg">
                        <h3 className="text-lg font-semibold mb-4">Crop Your Image</h3>
                        {sourceFile && (
                            <div className="flex justify-center">
                                <ReactCrop crop={crop} onChange={(_, percentCrop) => setCrop(percentCrop)} onComplete={(c) => setCompletedCrop(c)} aspect={aspect}>
                                    <img ref={imgRef} alt="Crop me" src={sourceFile} onLoad={onImageLoad} className="max-h-[60vh]" />
                                </ReactCrop>
                            </div>
                        )}
                        <div className="flex justify-end gap-4 mt-4">
                            <button onClick={() => setIsModalOpen(false)} className="py-2 px-4 border border-neutral-300 rounded-md text-sm">Cancel</button>
                            <button onClick={handleCrop} className="py-2 px-4 bg-rose-600 text-white rounded-md text-sm">Crop & Save</button>
                        </div>
                    </div>
                </div>
            )}
            <canvas ref={previewCanvasRef} style={{ display: 'none' }} />
        </>
    );
}