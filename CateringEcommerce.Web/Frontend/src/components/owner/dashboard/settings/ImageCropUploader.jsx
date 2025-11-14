/*
========================================
File: src/components/owner/dashboard/settings/ImageCropUploader.jsx (REVISED)
========================================
A reusable component for selecting and cropping an image, used for the logo.
*/
import React, { useState, useRef, forwardRef, useImperativeHandle } from 'react';
import ReactCrop, { centerCrop, makeAspectCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';

// A more robust function to create a high-quality cropped image at its true resolution.
async function getCroppedImg(image, crop, fileName) {
    const canvas = document.createElement('canvas');
    const scaleX = image.naturalWidth / image.width;
    const scaleY = image.naturalHeight / image.height;

    // Set canvas size to the actual pixel size of the crop on the original image.
    canvas.width = Math.floor(crop.width * scaleX);
    canvas.height = Math.floor(crop.height * scaleY);

    const ctx = canvas.getContext('2d');
    if (!ctx) {
        throw new Error('Could not get canvas context');
    }

    // Apply high-quality image smoothing
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = "high";

    const cropX = crop.x * scaleX;
    const cropY = crop.y * scaleY;

    // Draw the cropped portion of the original image onto our new canvas.
    ctx.drawImage(
        image,
        cropX,
        cropY,
        canvas.width, // Source width
        canvas.height, // Source height
        0,
        0,
        canvas.width, // Destination width
        canvas.height // Destination height
    );

    return new Promise((resolve, reject) => {
        canvas.toBlob(blob => {
            if (!blob) {
                reject(new Error('Canvas is empty.'));
                return;
            }
            blob.name = fileName;
            resolve(blob);
        }, 'image/jpeg', 0.95); // High quality JPEG output
    });
}


const ImageCropUploader = forwardRef(({ onCropComplete, aspect = 1 }, ref) => {
    const [imgSrc, setImgSrc] = useState('');
    const [crop, setCrop] = useState();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const imgRef = useRef(null);
    const fileInputRef = useRef(null);

    useImperativeHandle(ref, () => ({
        triggerFileSelect() {
            if (fileInputRef.current) {
                fileInputRef.current.click();
            }
        }
    }));

    const onSelectFile = (e) => {
        if (e.target.files && e.target.files.length > 0) {
            const reader = new FileReader();
            reader.addEventListener('load', () => setImgSrc(reader.result?.toString() || ''));
            reader.readAsDataURL(e.target.files[0]);
            setIsModalOpen(true);
        }
    };

    const onImageLoad = (e) => {
        const { width, height } = e.currentTarget;
        const crop = centerCrop(
            makeAspectCrop({ unit: 'px', width: Math.min(width, height) * 0.9 }, aspect, width, height),
            width,
            height
        );
        setCrop(crop);
    };

    const handleCrop = async () => {
        if (!imgRef.current || !crop?.width || !crop?.height) return;

        try {
            const croppedBlob = await getCroppedImg(imgRef.current, crop, 'cropped_logo.jpg');
            onCropComplete(croppedBlob);
        } catch (error) {
            console.error("Cropping failed:", error);
            // Optionally, show a toast message to the user
        } finally {
            // Close the modal regardless of success or failure
            setIsModalOpen(false);
            setImgSrc('');
            if (fileInputRef.current) {
                fileInputRef.current.value = "";
            }
        }
    };

    return (
        <>
            <input
                type="file"
                ref={fileInputRef}
                onChange={onSelectFile}
                className="hidden"
                accept="image/jpeg, image/png, image/gif, image/pjpeg, image/jfif"
            />
            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-75 flex justify-center items-center z-50 p-4">
                    <div className="bg-white rounded-lg p-6 max-w-lg w-full">
                        <h3 className="text-xl font-bold mb-4">Crop Your Logo</h3>
                        {imgSrc && (
                            <div className="flex justify-center">
                                <ReactCrop
                                    crop={crop}
                                    onChange={(pixelCrop, percentCrop) => setCrop(pixelCrop)}
                                    aspect={aspect}
                                    minWidth={100}
                                    minHeight={100}
                                    circularCrop={true}
                                >
                                    <img ref={imgRef} src={imgSrc} onLoad={onImageLoad} alt="Crop preview" style={{ maxHeight: '70vh' }} />
                                </ReactCrop>
                            </div>
                        )}
                        <div className="flex justify-end gap-3 mt-4">
                            <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 rounded-md text-sm font-medium text-neutral-700 bg-neutral-100 hover:bg-neutral-200">
                                Cancel
                            </button>
                            <button type="button" onClick={handleCrop} className="px-4 py-2 rounded-md text-sm font-medium text-white bg-rose-600 hover:bg-rose-700">
                                Crop & Save
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
});

export default ImageCropUploader;