
/*
========================================
File: src/components/owner/dashboard/decorations/DecorationCard.jsx (REVISED)
========================================
*/
import React from 'react';
import ToggleSwitch from '../../../common/ToggleSwitch';
import { ownerApiService } from '../../../../services/ownerApi';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function DecorationCard({ item, onEdit, onDelete, onStatusChange }) {
    const mainMedia = item.media[0];
    const mediaUrl = (media) => media.filePath ? `${API_BASE_URL}${media.filePath}` : media.preview;

    const handleToggle = (value) => {
        onStatusChange(item, value);
    };

    return (
        <div className="rounded-xl shadow-lg overflow-hidden bg-white transition-all duration-300 hover:shadow-2xl flex flex-col">
            <div className="relative">
                <div className="aspect-[4/3] w-full bg-neutral-200">
                    {ownerApiService.isImageType(mainMedia.mediaType)
                        ? <img src={mediaUrl(mainMedia)} alt={item.name} className="w-full h-full object-cover" />
                        : <video src={mediaUrl(mainMedia)} className="w-full h-full object-cover" />
                    }
                </div>

                {/* Action Icons */}
                <div className="absolute top-3 right-3 flex flex-col gap-2">
                    <button
                        onClick={onEdit}
                        title="Edit Setup"
                        className="w-8 h-8 flex items-center justify-center bg-white rounded-full shadow-md text-blue-600 hover:bg-blue-50 transition-all"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path d="M17.414 2.586a2 2 0 00-2.828 0L7 10.172V13h2.828l7.586-7.586a2 2 0 000-2.828z" />
                            <path fillRule="evenodd" d="M2 6a2 2 0 012-2h4a1 1 0 010 2H4v10h10v-4a1 1 0 112 0v4a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" clipRule="evenodd" />
                        </svg>
                    </button>
                    <button
                        onClick={onDelete}
                        title="Delete Setup"
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
                    <span className="text-xs font-semibold text-rose-500 uppercase tracking-wide">{item.theme}</span>
                    <h3 className="text-lg font-bold text-neutral-800 truncate mt-1">{item.name}</h3>
                    {item.price > 0 && (
                        <p className="text-lg font-bold text-neutral-900 mt-2">₹{item.price}</p>
                    )}
                    {item.linkedPackages && item.linkedPackages.length > 0 && (
                        <div className="mt-2" title={item.linkedPackages.map(p => p.name).join(', ')}>
                            <span className="flex items-center gap-1 text-xs font-medium text-blue-600">
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path d="M5 4a2 2 0 012-2h6a2 2 0 012 2v1H5V4zM5 7h10v9a2 2 0 01-2 2H7a2 2 0 01-2-2V7z" /></svg>
                                Linked to {item.linkedPackages.length} package(s)
                            </span>
                        </div>
                    )}
                </div>

                <div className="mt-4 pt-4 border-t">
                    <ToggleSwitch
                        label={item.status ? "Active" : "Inactive"}
                        enabled={item.status}
                        setEnabled={handleToggle}
                    />
                </div>
            </div>
        </div>
    );
}