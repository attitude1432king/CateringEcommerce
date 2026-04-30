/*
========================================
File: src/components/owner/dashboard/menu/FoodItemCard.jsx
========================================
Professional eCommerce-style food card
Edit/Delete icons appear on hover
*/

import React from "react";
import { ownerApiService } from "../../../../services/ownerApi";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

/* ---------------- Icons ---------------- */

const PackageIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
        <path
            fillRule="evenodd"
            d="M10 2a2 2 0 00-2 2v1H6a2 2 0 00-2 2v1H2a1 1 0 00-1 1v6a1 1 0 001 1h16a1 1 0 001-1V9a1 1 0 00-1-1h-2V7a2 2 0 00-2-2h-2V4a2 2 0 00-2-2h-2z"
            clipRule="evenodd"
        />
    </svg>
);

const SampleTasteIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
        <path d="M7 3a1 1 0 000 2h6a1 1 0 100-2H7z" />
        <path
            fillRule="evenodd"
            d="M2 8a3 3 0 013-3h10a3 3 0 013 3v8a3 3 0 01-3 3H5a3 3 0 01-3-3V8z"
            clipRule="evenodd"
        />
    </svg>
);

const EditIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
        <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
    </svg>
);

const DeleteIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
        <path
            fillRule="evenodd"
            d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9z"
            clipRule="evenodd"
        />
    </svg>
);

const DietIcon = ({ isVeg }) => (
    <div className={`border ${isVeg ? "border-green-600" : "border-red-600"} w-4 h-4 flex items-center justify-center rounded-sm bg-white`}>
        <div className={`w-2 h-2 rounded-full ${isVeg ? "bg-green-600" : "bg-red-600"}`} />
    </div>
);

/* ---------------- Component ---------------- */

export default function FoodItemCard({ item, onEdit, onDelete }) {
    const mainMedia = item.media?.[0];

    const mediaUrl = (media) =>
        media?.filePath ? `${API_BASE_URL}${media.filePath}` : media?.preview;

    return (
        <div className="rounded-xl shadow-lg bg-white overflow-hidden transition-all duration-300 hover:shadow-2xl flex flex-col group">

            {/* IMAGE SECTION */}
            <div className="relative">
                <div className="aspect-[4/3] w-full bg-neutral-200">
                    {ownerApiService.isImageType(mainMedia?.mediaType) ? (
                        <img
                            src={mediaUrl(mainMedia)}
                            alt={item.name}
                            className="w-full h-full object-cover"
                        />
                    ) : (
                        <video
                            src={mediaUrl(mainMedia)}
                            className="w-full h-full object-cover"
                        />
                    )}
                </div>

                {/* Overlay */}
                <div className="absolute inset-0 bg-gradient-to-t from-black/30 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

                {/* Status */}
                <div
                    className={`absolute top-3 left-3 px-2 py-0.5 rounded text-[10px] font-bold uppercase text-white ${item.status ? "bg-green-600" : "bg-neutral-500"
                        }`}
                >
                    {item.status ? "Active" : "Inactive"}
                </div>

                {/* ACTION BUTTONS */}
                <div className="absolute top-3 right-3 flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200 z-10">
                    <button
                        onClick={(e) => {
                            e.stopPropagation();
                            onEdit(item);
                        }}
                        className="w-8 h-8 bg-white rounded-full shadow-md flex items-center justify-center text-neutral-600 hover:text-blue-600 hover:bg-blue-50 transition"
                        title="Edit"
                    >
                        <EditIcon />
                    </button>

                    <button
                        onClick={(e) => {
                            e.stopPropagation();
                            onDelete(item);
                        }}
                        className="w-8 h-8 bg-white rounded-full shadow-md flex items-center justify-center text-neutral-600 hover:text-red-600 hover:bg-red-50 transition"
                        title="Delete"
                    >
                        <DeleteIcon />
                    </button>
                </div>
            </div>

            {/* CONTENT */}
            <div className="p-4 flex flex-col flex-grow">
                <div className="flex justify-between items-start mb-1">
                    <span className="text-[10px] font-bold text-neutral-400 uppercase">
                        {item.categoryName}
                    </span>
                    <DietIcon isVeg={item.isVeg} />
                </div>

                <h3 className="text-base font-bold text-neutral-800 line-clamp-1">
                    {item.name}
                </h3>

                <p className="text-sm text-neutral-500 line-clamp-2 mt-1">
                    {item.description}
                </p>

                {/* FOOTER */}
                <div className="mt-auto pt-4 border-t border-dashed border-neutral-100 flex justify-between items-center">
                    <p className="text-lg font-bold text-neutral-900">₹{item.price}</p>

                    <div className="flex gap-2">
                        {item.isLiveCounter && (
                            <span className="px-2 py-0.5 text-[10px] font-bold bg-amber-50 text-amber-700 rounded border border-amber-100">
                                🔥 Live
                            </span>
                        )}
                        {item.isSampleTaste && (
                            <span className="flex items-center gap-1 px-2 py-0.5 text-[10px] font-bold bg-teal-50 text-teal-700 rounded border border-teal-100">
                                <SampleTasteIcon /> Sample
                            </span>
                        )}
                        {item.isPackageItem && (
                            <span className="flex items-center gap-1 px-2 py-0.5 text-[10px] font-bold bg-blue-50 text-blue-700 rounded border border-blue-100">
                                <PackageIcon /> Package
                            </span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
