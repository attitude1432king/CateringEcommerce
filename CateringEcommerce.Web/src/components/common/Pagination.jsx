/*
========================================
File: src/components/common/Pagination.jsx (REVISED)
========================================
A reusable, eCommerce-style pagination component.
*/
import React from 'react';
import CustomSelectDropdown from './CustomSelectDropdown'; // Import the new component

// Reusable icon component
const ChevronIcon = ({ className }) => (
    <svg className={`h-5 w-5 ${className}`} xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
        <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
    </svg>
);

const PaginationButton = ({ onClick, disabled, children }) => (
    <button
        type="button"
        onClick={onClick}
        disabled={disabled}
        className="relative inline-flex items-center justify-center h-9 w-9 border border-neutral-300 bg-white text-sm font-medium text-neutral-700 rounded-md hover:bg-neutral-50 disabled:opacity-50 disabled:cursor-not-allowed"
    >
        {children}
    </button>
);

const perPageOptions = [
    { id: 10, name: '10' },
    { id: 20, name: '20' },
    { id: 50, name: '50' },
    { id: 100, name: '100' },
    { id: 200, name: '200' },
];

export default function Pagination({ currentPage, totalItems, itemsPerPage, onPageChange, onItemsPerPageChange }) {
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const startIndex = (currentPage - 1) * itemsPerPage + 1;
    const endIndex = Math.min(currentPage * itemsPerPage, totalItems);

    const handlePrev = () => {
        if (currentPage > 1) onPageChange(currentPage - 1);
    };
    const handleNext = () => {
        if (currentPage < totalPages) onPageChange(currentPage + 1);
    };

    if (totalItems === 0) return null;

    return (
        <div className="mt-6 flex flex-col md:flex-row justify-between items-center gap-4 text-sm font-medium text-neutral-600">
            {/* Left Side: Total Records & Per Page */}
            <div className="flex items-center gap-4">
                <p className="font-bold text-neutral-700">
                    Total {totalItems} items
                </p>
                <div className="flex items-center gap-2">
                    <span>Show:</span>
                    <div className="w-20">
                        <CustomSelectDropdown
                            options={perPageOptions}
                            value={itemsPerPage}
                            onChange={(newValue) => {
                                onItemsPerPageChange(newValue);
                                onPageChange(1); // Reset to page 1
                            }}
                        />
                    </div>
                </div>
            </div>

            {/* Right Side: Page Navigation */}
            {totalPages > 1 && (
                <div className="flex items-center gap-2">
                    <span className="font-semibold text-neutral-700">
                        {startIndex}-{endIndex} of {totalItems}
                    </span>
                    <div className="flex items-center gap-2">
                        <PaginationButton onClick={handlePrev} disabled={currentPage === 1}>
                            <ChevronIcon className="rotate-180" />
                        </PaginationButton>
                        <PaginationButton onClick={handleNext} disabled={currentPage === totalPages}>
                            <ChevronIcon />
                        </PaginationButton>
                    </div>
                </div>
            )}
        </div>
    );
}