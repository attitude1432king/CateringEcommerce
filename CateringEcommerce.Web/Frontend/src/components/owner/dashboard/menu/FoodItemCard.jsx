/*
========================================
File: src/components/owner/dashboard/menu/FoodItemCard.jsx (REVISED)
========================================
A professional, eCommerce-style card for displaying a food item.
No longer uses the expand/collapse logic.
*/
import React from 'react';
import { ownerApiService } from '../../../../services/ownerApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// YELLOW BOX: New SVG Icons
const PackageIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
        <path fillRule="evenodd" d="M10 2a2 2 0 00-2 2v1H6a2 2 0 00-2 2v1H2a1 1 0 00-1 1v6a1 1 0 001 1h16a1 1 0 001-1V9a1 1 0 00-1-1h-2V7a2 2 0 00-2-2h-2V4a2 2 0 00-2-2h-2zm-1 4v1h2V6H9zm2 3h2v1H9V9h2z" clipRule="evenodd" />
    </svg>
);

const SampleTasteIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
        <path d="M7 3a1 1 0 000 2h6a1 1 0 100-2H7z" />
        <path fillRule="evenodd" d="M2 8a3 3 0 013-3h10a3 3 0 013 3v8a3 3 0 01-3 3H5a3 3 0 01-3-3V8zm3 1a1 1 0 011-1h.01a1 1 0 110 2H6a1 1 0 01-1-1zm3 0a1 1 0 011-1h.01a1 1 0 110 2H9a1 1 0 01-1-1zm3 0a1 1 0 011-1h.01a1 1 0 110 2h-.01a1 1 0 01-1-1z" clipRule="evenodd" />
    </svg>
);


export default function FoodItemCard({ item, onEdit, onDelete }) {
    const mainMedia = item.media[0];
    const mediaUrl = (media) => media.filePath ? `${API_BASE_URL}${media.filePath}` : media.preview;

    return (
        <div className="rounded-xl shadow-lg overflow-hidden bg-white transition-all duration-300 hover:shadow-2xl flex flex-col">
            <div className="relative">
                {/* Media */}
                <div className="aspect-[4/3] w-full bg-neutral-200">
                    {ownerApiService.isImageType(mainMedia.mediaType)
                        ? <img src={mediaUrl(mainMedia)} alt={item.name} className="w-full h-full object-cover" />
                        : <video src={mediaUrl(mainMedia)} className="w-full h-full object-cover" />
                    }
                </div>

                {/* Status Badge */}
                <div className={`absolute top-3 left-3 px-2.5 py-1 rounded-full text-xs font-bold text-white ${item.status ? 'bg-green-600' : 'bg-neutral-500'}`}>
                    {item.status ? 'Active' : 'Inactive'}
                </div>

                {/* Action Icons */}
                <div className="absolute top-3 right-3 flex flex-col gap-2">
                    <button
                        onClick={onEdit}
                        title="Edit Item"
                        className="w-8 h-8 flex items-center justify-center bg-white rounded-full shadow-md text-blue-600 hover:bg-blue-50 transition-all"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path d="M17.414 2.586a2 2 0 00-2.828 0L7 10.172V13h2.828l7.586-7.586a2 2 0 000-2.828z" />
                            <path fillRule="evenodd" d="M2 6a2 2 0 012-2h4a1 1 0 010 2H4v10h10v-4a1 1 0 112 0v4a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" clipRule="evenodd" />
                        </svg>
                    </button>
                    <button
                        onClick={onDelete}
                        title="Delete Item"
                        className="w-8 h-8 flex items-center justify-center bg-white rounded-full shadow-md text-red-600 hover:bg-red-50 transition-all"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                    </button>
                </div>
            </div>

            <div className="p-4 flex flex-col flex-grow">
                <div className="flex-grow">
                    <div className="flex justify-between items-center">
                        <span className="text-xs font-semibold text-neutral-500 uppercase tracking-wide">{item.categoryName}</span>
                        {/* YELLOW BOX: Replaced text with icons and adjusted spacing */}
                        <div className="flex items-center gap-3">
                            {item.isSampleTaste && (
                                <span title="Sample taste available" className="flex items-center gap-1 text-xs font-bold text-teal-600">
                                    <SampleTasteIcon />
                                    Sample
                                </span>
                            )}
                            {item.isPackageItem && (
                                <span title="Included in packages" className="flex items-center gap-1 text-xs font-bold text-blue-600">
                                    <PackageIcon />
                                    Package
                                </span>
                            )}
                        </div>
                    </div>
                    <h3 className="text-lg font-bold text-neutral-800 truncate mt-1" title={item.name}>{item.name}</h3>
                    <p className="text-sm text-neutral-600 mt-1 h-10">{item.description}</p>
                </div>
                <div className="mt-4">
                    <p className="text-xl font-bold text-neutral-900">₹{item.price}</p>
                </div>
            </div>
        </div>
    );
}