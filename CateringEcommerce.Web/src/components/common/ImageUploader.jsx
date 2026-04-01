/*
========================================
File: src/components/common/ImageUploader.jsx (NEW FILE)
========================================
A reusable component for uploading and cropping images.
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

export default function ImageUploader({ onImageCropped, aspect = 1, circularCrop = false, triggerId }) {
    const [imgSrc, setImgSrc] = useState('');
    const [crop, setCrop] = useState();
    const [completedCrop, setCompletedCrop] = useState();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const imgRef = useRef(null);
    const previewCanvasRef = useRef(null);

    function onSelectFile(e) {
        if (e.target.files && e.target.files.length > 0) {
            setCrop(undefined);
            const reader = new FileReader();
            reader.addEventListener('load', () => {
                setImgSrc(reader.result?.toString() || '');
                setIsModalOpen(true);
            });
            reader.readAsDataURL(e.target.files[0]);
            e.target.value = null; // Reset input value
        }
    }

    function onImageLoad(e) {
        const { width, height } = e.currentTarget;
        setCrop(centerAspectCrop(width, height, aspect));
    }

    async function handleCrop() {
        if (!previewCanvasRef.current || !imgRef.current || !completedCrop) {
            return;
        }
        const image = imgRef.current;
        const canvas = previewCanvasRef.current;
        const scaleX = image.naturalWidth / image.width;
        const scaleY = image.naturalHeight / image.height;
        canvas.width = completedCrop.width * scaleX;
        canvas.height = completedCrop.height * scaleY;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(image, completedCrop.x * scaleX, completedCrop.y * scaleY, completedCrop.width * scaleX, completedCrop.height * scaleY, 0, 0, canvas.width, canvas.height);
        canvas.toBlob((blob) => {
            if (blob) onImageCropped(blob);
            setIsModalOpen(false);
            setImgSrc('');
        }, 'image/jpeg', 0.92);
        return;
    }

    return (
        <>
            <input type="file" accept="image/jpeg, image/png, image/jpg" onChange={onSelectFile} className="hidden" id={triggerId} />

            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
                    <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-lg">
                        <h3 className="text-lg font-semibold mb-4">Crop Your Image</h3>
                        {imgSrc && (
                            <div className="flex justify-center">
                                <ReactCrop crop={crop} onChange={(_, percentCrop) => setCrop(percentCrop)} onComplete={(c) => setCompletedCrop(c)} aspect={aspect} circularCrop={circularCrop}>
                                    <img ref={imgRef} alt="Crop me" src={imgSrc} onLoad={onImageLoad} className="max-h-[60vh]" />
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