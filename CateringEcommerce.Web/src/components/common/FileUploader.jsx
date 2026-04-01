import React, { useState, useRef } from "react";
import ReactCrop from "react-image-crop";
import "react-image-crop/dist/ReactCrop.css";

export default function FileUploader({
    onFileCropped,
    aspect,
    acceptedTypes = "image/jpeg, image/png, application/pdf",
}) {
    const [sourceFile, setSourceFile] = useState(null);
    const [originalFile, setOriginalFile] = useState(null);
    const [crop, setCrop] = useState();
    const [completedCrop, setCompletedCrop] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const imgRef = useRef(null);
    const canvasRef = useRef(null);
    const fileInputRef = useRef(null);

    function onSelectFile(e) {
        if (!e.target.files?.length) return;

        const file = e.target.files[0];
        setOriginalFile(file);

        if (file.type.startsWith("image/")) {
            const reader = new FileReader();
            reader.onload = () => {
                setSourceFile(reader.result?.toString() || "");
                setIsModalOpen(true);
            };
            reader.readAsDataURL(file);
        } else {
            // Non-image (PDF etc.) — return immediately without cropping
            onFileCropped({
                file,
                name: file.name,
                type: file.type,
                previewUrl: null,
            });
        }
    }

    function handleCropSave() {
        if (!completedCrop || !imgRef.current || !canvasRef.current) return;

        const image = imgRef.current;
        const canvas = canvasRef.current;

        const scaleX = image.naturalWidth / image.width;
        const scaleY = image.naturalHeight / image.height;

        canvas.width = completedCrop.width * scaleX;
        canvas.height = completedCrop.height * scaleY;

        const ctx = canvas.getContext("2d");
        ctx.drawImage(
            image,
            completedCrop.x * scaleX,
            completedCrop.y * scaleY,
            completedCrop.width * scaleX,
            completedCrop.height * scaleY,
            0,
            0,
            canvas.width,
            canvas.height
        );

        canvas.toBlob((blob) => {
            if (!blob) return;
            const fileName = originalFile?.name || "cropped_image.jpg";
            const croppedFile = new File([blob], fileName, { type: "image/jpeg" });
            onFileCropped({
                file: croppedFile,
                name: croppedFile.name,
                type: croppedFile.type,
                previewUrl: URL.createObjectURL(croppedFile),
            });
            reset();
        }, "image/jpeg");
    }

    function handleUseOriginal() {
        if (originalFile) {
            onFileCropped({
                file: originalFile,
                name: originalFile.name,
                type: originalFile.type,
                previewUrl: originalFile.type.startsWith("image/")
                    ? URL.createObjectURL(originalFile)
                    : null,
            });
        }
        reset();
    }

    function handleCancel() {
        reset();
    }

    function reset() {
        setIsModalOpen(false);
        setSourceFile(null);
        setOriginalFile(null);
        setCrop(undefined);
        setCompletedCrop(null);
        fileInputRef.current.value = "";
    }

    return (
        <>
            <input
                type="file"
                accept={acceptedTypes}
                onChange={onSelectFile}
                ref={fileInputRef}
                className="hidden"
            />

            <button
                type="button"
                onClick={() => fileInputRef.current.click()}
                className="w-full text-sm text-rose-600 font-semibold hover:underline"
            >
                Click to upload
            </button>

            {isModalOpen && (
                <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
                    <div className="bg-white p-6 rounded-lg shadow-xl w-full max-w-xl">
                        <h3 className="text-lg font-semibold mb-4">Crop Your Image</h3>

                        <div className="flex justify-center">
                            <ReactCrop
                                crop={crop}
                                onChange={(c) => setCrop(c)}
                                onComplete={(c) => setCompletedCrop(c)}
                                aspect={aspect}
                            >
                                <img
                                    ref={imgRef}
                                    src={sourceFile}
                                    alt="Crop"
                                    className="max-h-[60vh]"
                                />
                            </ReactCrop>
                        </div>

                        <div className="flex justify-between mt-6">
                            <button
                                type="button"
                                onClick={handleUseOriginal}
                                className="py-2 px-4 border rounded-md text-sm"
                            >
                                Use Original
                            </button>

                            <div className="flex gap-4">
                                <button
                                    type="button"
                                    onClick={handleCancel}
                                    className="py-2 px-4 border rounded-md text-sm"
                                >
                                    Cancel
                                </button>

                                <button
                                    type="button"
                                    onClick={handleCropSave}
                                    className="py-2 px-4 bg-rose-600 text-white rounded-md text-sm"
                                >
                                    Crop & Save
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            <canvas ref={canvasRef} style={{ display: "none" }} />
        </>
    );
}
