/*
========================================
File: src/components/AppHeader.jsx
========================================
*/

import React, { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../../contexts/AuthContext';

// Utility function to generate an avatar from initials
const generateInitialsAvatar = (name) => {
    if (!name) return 'https://placehold.co/64x64/E0E7FF/4338CA?text=Q';
    const initials = name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64">
                    <circle cx="32" cy="32" r="32" fill="#e0e7ff"/>
                    <text x="50%" y="50%" dominant-baseline="central" text-anchor="middle" font-family="sans-serif" font-size="28" fill="#4338ca">${initials}</text>
                 </svg>`;
    return `data:image/svg+xml;base64,${btoa(svg)}`;
};

export default function AppHeader({ onOpenAuthModal, navigateTo }) {

    const { isAuthenticated, user, logout } = useAuth();
    const dropdownRef = useRef(null);
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);

    const handleLogout = () => {
        logout();
        navigateTo('home'); // Navigate to home on logout
    };

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setIsDropdownOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [dropdownRef]);


    const userAvatar = isAuthenticated
        ? user.profilePhoto || generateInitialsAvatar(user.name)
        : null;
   
    return (
        <header className="bg-white shadow-md sticky top-0 z-50 w-full">
            <div className="w-full max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8 min-w-0">
                <div className="flex items-center justify-between h-16 min-w-0">

                    {/* Logo */}
                    <div className="flex items-center min-w-0">
                        <div className="flex-shrink-0">
                            <Link to="/" className="text-3xl font-bold text-rose-600 flex items-center">
                                <span className="icon-placeholder text-3xl mr-1">🍽️</span>
                                Feasto
                            </Link>
                        </div>
                    </div>

                    {/* Center Inputs */}
                    <div className="hidden md:flex items-center space-x-4 flex-grow justify-center min-w-0">

                        {/* Location */}
                        <div className="relative min-w-0">
                            <input
                                type="text"
                                placeholder="Enter your location"
                                className="pl-10 pr-4 py-2 border border-neutral-300 rounded-md focus:ring-rose-500 focus:border-rose-500 sm:text-sm w-full"
                            />
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <svg className="h-5 w-5 text-neutral-400" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                                </svg>
                            </div>
                        </div>

                        {/* Search Bar */}
                        <div className="relative flex-grow max-w-xs lg:max-w-md min-w-0">
                            <input
                                type="search"
                                placeholder="Search for caterers, cuisines..."
                                className="w-full pl-10 pr-4 py-2 border border-neutral-300 rounded-md 
                                   focus:ring-rose-500 focus:border-rose-500 sm:text-sm"
                            />
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <svg className="h-5 w-5 text-neutral-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                                </svg>
                            </div>
                        </div>

                        <a href="#" className="text-neutral-600 hover:text-rose-600 px-3 py-2 rounded-md text-sm font-medium">
                            Offers
                        </a>
                    </div>

                    {/* Right Section */}
                    <div className="flex items-center space-x-4 min-w-0">

                        {!isAuthenticated && (
                            <Link to="/partner-login" className="hidden md:block text-neutral-600 hover:text-rose-600 px-3 py-2 rounded-md text-sm font-medium">
                                Become a Partner
                            </Link>
                        )}

                        {/* Avatar + Dropdown */}
                        {isAuthenticated ? (
                            <div className="relative" ref={dropdownRef}>
                                <button onClick={() => setIsDropdownOpen(!isDropdownOpen)} className="flex items-center">
                                    <img src={userAvatar} alt="User Avatar" className="h-9 w-9 rounded-full object-cover bg-amber-100 ring-2 ring-offset-2 ring-rose-500" />
                                </button>

                                {isDropdownOpen && (
                                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-20">
                                        <div className="px-4 py-2 border-b">
                                            <p className="text-sm font-semibold text-neutral-800 truncate">{user.name}</p>
                                        </div>
                                        <Link to="/profile" onClick={() => setIsDropdownOpen(false)} className="block px-4 py-2 text-sm hover:bg-amber-100 hover:text-rose-600">My Profile</Link>
                                        <button onClick={() => { handleLogout(); setIsDropdownOpen(false); }} className="block w-full text-left px-4 py-2 text-sm hover:bg-amber-100 hover:text-rose-600">Logout</button>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <button onClick={onOpenAuthModal} className="bg-rose-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-rose-700">
                                Login / Sign Up
                            </button>
                        )}
                    </div>

                </div>
            </div>
        </header>

    );
}