/**
 * EnhancedCartDrawer Component
 *
 * Swiggy/Zomato style floating cart with authentication guard
 * Shows auth modal when user tries to checkout without login
 */

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuthGuard } from '../../hooks/useAuthGuard';
import AuthModal from './AuthModal';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

export default function EnhancedCartDrawer() {
    const { cart, clearCart, isCartOpen, setIsCartOpen } = useCart();
    const { requireAuth, showAuthModal, handleAuthClose, isAuthenticated } = useAuthGuard();
    const navigate = useNavigate();
    const [isClosing, setIsClosing] = useState(false);
    const [showClearConfirm, setShowClearConfirm] = useState(false);

    // Auto-close animation
    const handleClose = () => {
        setIsClosing(true);
        setTimeout(() => {
            setIsCartOpen(false);
            setIsClosing(false);
        }, 300);
    };

    // Handle clear cart confirmation
    const handleClearCartConfirm = () => {
        clearCart();
        setShowClearConfirm(false);
        handleClose();
    };

    // Close on Escape key
    useEffect(() => {
        const handleEscape = (e) => {
            if (e.key === 'Escape' && isCartOpen) {
                handleClose();
            }
        };
        window.addEventListener('keydown', handleEscape);
        return () => window.removeEventListener('keydown', handleEscape);
    }, [isCartOpen]);

    // Lock page scroll while cart drawer or confirmation modal is open
    useEffect(() => {
        if (!isCartOpen && !showClearConfirm) {
            return undefined;
        }

        const scrollY = window.scrollY;
        const originalBodyOverflow = document.body.style.overflow;
        const originalBodyPosition = document.body.style.position;
        const originalBodyTop = document.body.style.top;
        const originalBodyWidth = document.body.style.width;
        const originalHtmlOverflow = document.documentElement.style.overflow;

        document.body.style.overflow = 'hidden';
        document.body.style.position = 'fixed';
        document.body.style.top = `-${scrollY}px`;
        document.body.style.width = '100%';
        document.documentElement.style.overflow = 'hidden';

        return () => {
            document.body.style.overflow = originalBodyOverflow;
            document.body.style.position = originalBodyPosition;
            document.body.style.top = originalBodyTop;
            document.body.style.width = originalBodyWidth;
            document.documentElement.style.overflow = originalHtmlOverflow;
            window.scrollTo(0, scrollY);
        };
    }, [isCartOpen, showClearConfirm]);

    
    /**
     * Handle "Proceed to Checkout" click
     * Check authentication first, then navigate
     */
    const handleProceedToCheckout = () => {
        // Close cart drawer
        handleClose();

        // Check auth and navigate
        requireAuth(
            // Action to perform after auth
            () => {
                navigate('/checkout');
            },
            // Redirect path
            '/checkout'
        );
    };

    if (!isCartOpen || !cart) return null;

    const {
        cateringName,
        cateringLogo,
        packageName,
        guestCount = 50,
        additionalItems = [],
        baseAmount = 0,
        taxAmount = 0,
        totalAmount = 0
    } = cart;

    return (
        <>
            {/* Backdrop Overlay */}
            <div
                className={`fixed inset-0 bg-black/60 backdrop-blur-sm z-[9998] transition-opacity duration-300 ${
                    isClosing ? 'opacity-0' : 'opacity-100'
                }`}
                onClick={handleClose}
            />

            {/* Cart Drawer */}
            <div
                className={`fixed right-0 top-0 h-full w-full sm:w-[480px] bg-white shadow-2xl z-[9999] flex flex-col transition-transform duration-300 ease-out ${
                    isClosing ? 'translate-x-full' : 'translate-x-0'
                }`}
            >
                {/* Header */}
                <div className="bg-gradient-to-r from-rose-500 to-rose-600 p-4 sm:p-6 text-white shadow-lg">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="bg-white/20 backdrop-blur-sm p-2 rounded-full">
                                <svg
                                    className="w-6 h-6"
                                    fill="none"
                                    stroke="currentColor"
                                    viewBox="0 0 24 24"
                                >
                                    <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth={2}
                                        d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"
                                    />
                                </svg>
                            </div>
                            <div>
                                <h2 className="text-xl sm:text-2xl font-bold">Your Cart</h2>
                                <p className="text-rose-100 text-sm">
                                    {isAuthenticated ? 'Review your order' : 'Sign in to checkout'}
                                </p>
                            </div>
                        </div>
                        <button
                            onClick={handleClose}
                            className="p-2 hover:bg-white/20 rounded-full transition-all"
                        >
                            <svg
                                className="w-6 h-6"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M6 18L18 6M6 6l12 12"
                                />
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Cart Content - Scrollable */}
                <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6">
                    {/* Caterer Info */}
                    <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-2xl p-4 border border-gray-200 shadow-sm">
                        <div className="flex items-center gap-4">
                            <div className="w-16 h-16 rounded-xl overflow-hidden bg-white shadow-md flex-shrink-0">
                                {cateringLogo ? (
                                    <img
                                        src={`${API_BASE_URL}${cateringLogo}`}
                                        alt={cateringName}
                                        className="w-full h-full object-cover"
                                    />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-rose-400 to-rose-600 text-white text-xl font-bold">
                                        {cateringName?.charAt(0)}
                                    </div>
                                )}
                            </div>
                            <div className="flex-1">
                                <h3 className="font-bold text-gray-900 text-lg">{cateringName}</h3>
                                <p className="text-sm text-gray-600 flex items-center gap-1 mt-1">
                                    <svg className="w-4 h-4 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                                        <path
                                            fillRule="evenodd"
                                            d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                                            clipRule="evenodd"
                                        />
                                    </svg>
                                    Verified Caterer
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Package Details */}
                    <div className="space-y-3">
                        <h4 className="font-semibold text-gray-900 flex items-center gap-2">
                            <span className="w-1.5 h-5 bg-rose-500 rounded-full"></span>
                            Package Details
                        </h4>
                        <div className="bg-white rounded-xl border-2 border-gray-200 p-4 space-y-3">
                            <div className="flex justify-between items-start">
                                <div className="flex-1">
                                    <p className="font-medium text-gray-900">{packageName}</p>
                                    <p className="text-sm text-gray-600 mt-1">
                                        for {guestCount} guests
                                    </p>
                                </div>
                                <div className="text-right">
                                    <p className="font-bold text-gray-900 text-lg">
                                        ₹{baseAmount.toLocaleString()}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Additional Items */}
                    {additionalItems && additionalItems.length > 0 && (
                        <div className="space-y-3">
                            <h4 className="font-semibold text-gray-900 flex items-center gap-2">
                                <span className="w-1.5 h-5 bg-amber-500 rounded-full"></span>
                                Add-ons
                            </h4>
                            <div className="space-y-2">
                                {additionalItems.map((item, index) => (
                                    <div
                                        key={index}
                                        className="bg-white rounded-xl border border-gray-200 p-3 flex justify-between items-center"
                                    >
                                        <div className="flex items-center gap-3">
                                            <div className="w-12 h-12 rounded-lg bg-gradient-to-br from-amber-400 to-amber-600 flex items-center justify-center text-white">
                                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                                                </svg>
                                            </div>
                                            <div>
                                                <p className="font-medium text-gray-900 text-sm">
                                                    {item.name}
                                                </p>
                                                <p className="text-xs text-gray-500">
                                                    Qty: {item.quantity} × {guestCount} guests
                                                </p>
                                            </div>
                                        </div>
                                        <p className="font-semibold text-gray-900">
                                            ₹{(item.price * item.quantity * guestCount).toLocaleString()}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Bill Summary */}
                    <div className="space-y-3">
                        <h4 className="font-semibold text-gray-900 flex items-center gap-2">
                            <span className="w-1.5 h-5 bg-blue-500 rounded-full"></span>
                            Bill Summary
                        </h4>
                        <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl border border-gray-200 p-4 space-y-3">
                            <div className="flex justify-between text-gray-700">
                                <span>Subtotal</span>
                                <span className="font-medium">₹{(totalAmount - taxAmount).toLocaleString()}</span>
                            </div>
                            <div className="flex justify-between text-gray-700">
                                <span className="flex items-center gap-1">
                                    GST (18%)
                                    <svg className="w-4 h-4 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                                        <path
                                            fillRule="evenodd"
                                            d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                                            clipRule="evenodd"
                                        />
                                    </svg>
                                </span>
                                <span className="font-medium">₹{taxAmount.toLocaleString()}</span>
                            </div>
                            <div className="pt-3 border-t-2 border-dashed border-gray-300 flex justify-between">
                                <span className="text-lg font-bold text-gray-900">Total Amount</span>
                                <span className="text-lg font-bold text-rose-600">
                                    ₹{totalAmount.toLocaleString()}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Authentication Notice (if not logged in) */}
                    {!isAuthenticated && (
                        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-2 border-blue-200 rounded-xl p-4 flex items-start gap-3">
                            <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
                                <svg className="w-5 h-5 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                                    <path
                                        fillRule="evenodd"
                                        d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                                        clipRule="evenodd"
                                    />
                                </svg>
                            </div>
                            <div className="flex-1">
                                <p className="font-semibold text-blue-900 mb-1">Sign in Required</p>
                                <p className="text-sm text-blue-700">
                                    You'll be asked to sign in to proceed with your order
                                </p>
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer Actions */}
                <div className="border-t-2 border-gray-200 bg-white p-4 sm:p-6 space-y-3 shadow-2xl">
                    {/* Total Display */}
                    <div className="flex items-center justify-between px-2">
                        <div>
                            <p className="text-sm text-gray-600">Total Amount</p>
                            <p className="text-2xl font-bold text-gray-900">
                                ₹{totalAmount.toLocaleString()}
                            </p>
                        </div>
                    </div>

                    {/* Proceed Button */}
                    <button
                        onClick={handleProceedToCheckout}
                        className="w-full bg-gradient-to-r from-rose-500 to-rose-600 hover:from-rose-600 hover:to-rose-700 text-white py-4 rounded-xl font-bold text-lg shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-[1.02] active:scale-[0.98] flex items-center justify-center gap-3"
                    >
                        <span>Proceed to Checkout</span>
                        <svg
                            className="w-6 h-6"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                        >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M13 7l5 5m0 0l-5 5m5-5H6"
                            />
                        </svg>
                    </button>

                    {/* Clear Cart */}
                    <button
                        onClick={() => setShowClearConfirm(true)}
                        className="w-full text-red-600 hover:text-red-700 py-2 text-sm font-medium transition-colors"
                    >
                        Clear Cart
                    </button>
                </div>
            </div>

            {/* Auth Modal */}
            {showAuthModal && (
                <AuthModal
                    isOpen={showAuthModal}
                    onClose={handleAuthClose}
                    isPartnerLogin={false}
                />
            )}

            {/* Clear Cart Confirmation Modal */}
            {showClearConfirm && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[10001] flex items-center justify-center p-4">
                    <div
                        className="bg-white rounded-xl shadow-2xl max-w-sm w-full p-6 animate-fade-in"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="text-center mb-6">
                            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
                                <svg className="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900 mb-2">Clear Cart?</h3>
                            <p className="text-sm text-gray-600">
                                Are you sure you want to remove all items from your cart? This action cannot be undone.
                            </p>
                        </div>

                        <div className="flex gap-3">
                            <button
                                onClick={() => setShowClearConfirm(false)}
                                className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleClearCartConfirm}
                                className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-medium"
                            >
                                Clear Cart
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
