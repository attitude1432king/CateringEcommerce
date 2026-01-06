import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../contexts/AuthContext';

const generateInitialsAvatar = (name) => {
    if (!name) return 'https://placehold.co/64x64/FF6B35/FFFFFF?text=Q';
    const initials = name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><circle cx="32" cy="32" r="32" fill="#FF6B35"/><text x="50%" y="50%" dominant-baseline="central" text-anchor="middle" font-family="sans-serif" font-size="28" fill="white">${initials}</text></svg>`;
    return `data:image/svg+xml;base64,${btoa(svg)}`;
};

export default function AppHeader({ onOpenAuthModal }) {
    const { isAuthenticated, user, logout } = useAuth();
    const dropdownRef = useRef(null);
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setIsDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleLogout = () => {
        logout();
        navigate('/');
        setIsDropdownOpen(false);
    };

    const userAvatar = isAuthenticated ? (user?.profilePhoto || generateInitialsAvatar(user?.name)) : null;

    return (
        <header className="bg-white shadow-sm sticky top-0 z-50 w-full border-b border-neutral-100">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16 md:h-20">
                    {/* Left: Logo */}
                    <Link to="/" className="flex items-center gap-2 flex-shrink-0">
                        <div className="text-3xl">???</div>
                        <span className="font-bold text-xl md:text-2xl bg-gradient-catering bg-clip-text text-transparent hidden sm:inline-block">
                            Feasto
                        </span>
                    </Link>

                    {/* Center: Navigation - Desktop Only */}
                    <nav className="hidden md:flex items-center space-x-2">
                        <Link to="/events" className="px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            Events
                        </Link>
                        <Link to="/corporate" className="px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            Services
                        </Link>
                        <a href="#how-it-works" className="px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            How it Works
                        </a>
                    </nav>

                    {/* Right: Actions */}
                    <div className="flex items-center space-x-3 flex-shrink-0">
                        {/* Become a Partner - Desktop */}
                        {!isAuthenticated && (
                            <Link
                                to="/partner-login"
                                className="hidden lg:inline-flex px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors"
                            >
                                Partner With Us
                            </Link>
                        )}

                        {/* Get Quotes Button */}
                        <button
                            type="button"
                            className="hidden sm:inline-flex btn-primary px-6 py-2 md:py-3 gap-2 text-sm md:text-base"
                            onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                        >
                            <svg className="w-4 h-4 md:w-5 md:h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                            </svg>
                            <span className="hidden md:inline">Get Quotes</span>
                        </button>

                        {/* User Account / Auth */}
                        {isAuthenticated ? (
                            <div className="relative" ref={dropdownRef}>
                                <button
                                    onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                                    aria-haspopup="true"
                                    aria-expanded={isDropdownOpen}
                                    className="focus:outline-none"
                                >
                                    <img
                                        src={userAvatar}
                                        alt="User Avatar"
                                        className="h-10 w-10 rounded-full object-cover ring-2 ring-offset-1 ring-catering-primary hover:ring-catering-primary-dark transition-all"
                                    />
                                </button>

                                {isDropdownOpen && (
                                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg py-2 z-20 border border-neutral-200">
                                        <div className="px-4 py-3 border-b border-neutral-200">
                                            <p className="font-semibold text-neutral-900 text-sm">{user?.name || 'User'}</p>
                                            <p className="text-xs text-neutral-500">{user?.email}</p>
                                        </div>
                                        <Link to="/profile" onClick={() => setIsDropdownOpen(false)} className="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-100 transition-colors">
                                            My Profile
                                        </Link>
                                        <Link to="/bookings" onClick={() => setIsDropdownOpen(false)} className="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-100 transition-colors">
                                            My Bookings
                                        </Link>
                                        <button
                                            onClick={handleLogout}
                                            className="block w-full text-left px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-100 transition-colors border-t border-neutral-200"
                                        >
                                            Logout
                                        </button>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <button
                                onClick={onOpenAuthModal}
                                className="btn-primary px-4 md:px-6 py-2 md:py-3 text-sm md:text-base font-semibold"
                            >
                                Login
                            </button>
                        )}

                        {/* Mobile Menu Toggle */}
                        <button
                            type="button"
                            className="md:hidden p-2 text-neutral-700 hover:bg-neutral-100 rounded-lg transition-colors"
                            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                            aria-label="Toggle menu"
                        >
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Mobile Menu - Conditionally Rendered */}
                {isMobileMenuOpen && (
                    <div className="md:hidden border-t border-neutral-100 py-4 space-y-2">
                        <Link to="/events" className="block px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            Events
                        </Link>
                        <Link to="/corporate" className="block px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            Corporate Catering
                        </Link>
                        <a href="#how-it-works" className="block px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors">
                            How it Works
                        </a>
                        {!isAuthenticated && (
                            <Link
                                to="/partner-login"
                                className="block px-4 py-2 text-neutral-700 font-medium rounded-lg hover:bg-neutral-100 transition-colors"
                            >
                                Become a Partner
                            </Link>
                        )}
                    </div>
                )}
            </div>
        </header>
    );
}