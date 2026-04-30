/*
========================================
File: src/components/user/CartDrawer.jsx (REFACTORED - DRAWER OPTIMIZED)
========================================
Cart drawer component optimized for compact sidebar display.
*/
import React, { useState } from 'react';
import { useCart } from '../../contexts/CartContext';
import { useNavigate } from 'react-router-dom';
import { useAppSettings } from '../../contexts/AppSettingsContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default function CartDrawer() {
    const { cart, isCartOpen, setIsCartOpen, clearCart, updateCart, removeAdditionalItem } = useCart();
    const navigate = useNavigate();
    const { getInt } = useAppSettings();
    const [showClearConfirm, setShowClearConfirm] = useState(false);

    if (!isCartOpen) return null;

    const handleClose = () => setIsCartOpen(false);

    const handleClearCartConfirm = () => {
        clearCart();
        setShowClearConfirm(false);
    };

    const handleGuestCountChange = (newCount) => {
        if (newCount >= 50 && newCount <= 10000) {
            updateCart({ guestCount: newCount });
        }
    };

    const handleProceedToCheckout = () => {
        setIsCartOpen(false);
        navigate('/checkout');
    };

    const handleViewFullCart = () => {
        setIsCartOpen(false);
        navigate('/cart');
    };

    const handleBrowseCatering = () => {
        setIsCartOpen(false);
        navigate('/caterings');
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('en-IN', {
            style: 'currency',
            currency: 'INR',
            maximumFractionDigits: 0
        }).format(amount || 0);
    };

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
                onClick={handleClose}
            />

            {/* Drawer */}
            <div className="fixed right-0 top-0 h-full w-full sm:w-96 md:w-[420px] bg-white shadow-2xl z-50 flex flex-col animate-slide-in-right">
                {/* Header */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-neutral-200 bg-gradient-to-r from-orange-50 to-red-50">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-orange-500 rounded-lg flex items-center justify-center">
                            <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                        </div>
                        <div>
                            <h2 className="text-xl font-bold text-neutral-900">Your Cart</h2>
                            {cart && <p className="text-xs text-neutral-600">{cart.cateringName}</p>}
                        </div>
                    </div>
                    <button
                        onClick={handleClose}
                        className="p-2 hover:bg-white rounded-full transition-colors"
                        aria-label="Close cart"
                    >
                        <svg className="w-6 h-6 text-neutral-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Content */}
                {!cart ? (
                    /* Empty Cart State */
                    <div className="flex-1 flex flex-col items-center justify-center px-6 py-12 text-center">
                        <div className="w-32 h-32 bg-gradient-to-br from-orange-100 to-red-100 rounded-full flex items-center justify-center mb-6">
                            <svg className="w-16 h-16 text-orange-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                        </div>
                        <h3 className="text-2xl font-bold text-neutral-900 mb-2">Your cart is empty</h3>
                        <p className="text-sm text-neutral-600 mb-8 max-w-xs">
                            Discover amazing catering services for your events. Start adding delicious packages!
                        </p>
                        <button
                            onClick={handleBrowseCatering}
                            className="btn-primary px-8 py-3 text-base font-semibold flex items-center gap-2"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                            Browse Catering
                        </button>

                        {/* Popular Searches */}
                        <div className="mt-12 w-full max-w-sm">
                            <p className="text-xs text-neutral-500 mb-4 uppercase tracking-wide font-medium">Popular Searches</p>
                            <div className="flex flex-wrap gap-2 justify-center">
                                <button onClick={handleBrowseCatering} className="px-4 py-2 bg-gray-100 hover:bg-orange-100 text-gray-700 hover:text-orange-700 rounded-full text-sm transition-colors">
                                    Wedding Catering
                                </button>
                                <button onClick={handleBrowseCatering} className="px-4 py-2 bg-gray-100 hover:bg-orange-100 text-gray-700 hover:text-orange-700 rounded-full text-sm transition-colors">
                                    Corporate Events
                                </button>
                                <button onClick={handleBrowseCatering} className="px-4 py-2 bg-gray-100 hover:bg-orange-100 text-gray-700 hover:text-orange-700 rounded-full text-sm transition-colors">
                                    Birthday Parties
                                </button>
                            </div>
                        </div>
                    </div>
                ) : (
                    <>
                        {/* Cart Content - Scrollable */}
                        <div className="flex-1 overflow-y-auto px-6 py-4">
                            {/* Caterer Info */}
                            <div className="mb-4 pb-4 border-b border-neutral-200">
                                <div className="flex items-start gap-3">
                                    {cart.cateringLogo && (
                                        <img
                                            src={`${API_BASE_URL}${cart.cateringLogo}`}
                                            alt={cart.cateringName}
                                            className="w-14 h-14 rounded-lg object-cover border border-neutral-200"
                                        />
                                    )}
                                    <div className="flex-1">
                                        <h3 className="text-base font-bold text-neutral-900">{cart.cateringName}</h3>
                                        <p className="text-xs text-neutral-500">Bulk Catering Service</p>
                                    </div>
                                </div>

                                {/* Package Info */}
                                {cart.packageName && (
                                    <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mt-3">
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <p className="text-sm font-semibold text-amber-900">{cart.packageName}</p>
                                                <p className="text-xs text-amber-700">{formatCurrency(cart.packagePrice)} per plate</p>
                                            </div>
                                            <svg className="w-5 h-5 text-amber-600" fill="currentColor" viewBox="0 0 20 20">
                                                <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                            </svg>
                                        </div>
                                    </div>
                                )}
                            </div>

                            {/* Compact Guest Count Selector */}
                            <div className="mb-4 pb-4 border-b border-neutral-200">
                                <label className="block text-sm font-semibold text-neutral-900 mb-2">
                                    Number of Guests
                                </label>
                                <div className="flex items-center gap-2">
                                    <button
                                        onClick={() => handleGuestCountChange(cart.guestCount - 10)}
                                        className="w-10 h-10 flex items-center justify-center bg-white border-2 border-neutral-300 rounded-lg hover:border-orange-500 hover:bg-orange-50 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                                        disabled={cart.guestCount <= 50}
                                    >
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
                                        </svg>
                                    </button>
                                    <input
                                        type="number"
                                        value={cart.guestCount}
                                        onChange={(e) => handleGuestCountChange(parseInt(e.target.value) || 50)}
                                        className="flex-1 text-center px-3 py-2 border-2 border-neutral-300 rounded-lg font-bold text-lg text-neutral-900 focus:outline-none focus:border-orange-500"
                                        min="50"
                                        max="10000"
                                    />
                                    <button
                                        onClick={() => handleGuestCountChange(cart.guestCount + 10)}
                                        className="w-10 h-10 flex items-center justify-center bg-white border-2 border-neutral-300 rounded-lg hover:border-orange-500 hover:bg-orange-50 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                                        disabled={cart.guestCount >= 10000}
                                    >
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                                        </svg>
                                    </button>
                                </div>
                                <p className="text-xs text-neutral-500 mt-2 text-center">Min: {getInt('BUSINESS.MIN_GUESTS_PER_ORDER', 50)}, Max: 10,000 guests</p>
                            </div>

                            {/* Decoration (if selected) */}
                            {cart.decorationName && (
                                <div className="mb-4 pb-4 border-b border-neutral-200">
                                    <h4 className="text-sm font-semibold text-neutral-900 mb-2">Decoration Theme</h4>
                                    <div className="flex items-center justify-between bg-purple-50 border border-purple-200 rounded-lg p-3">
                                        <p className="text-sm font-medium text-purple-900">{cart.decorationName}</p>
                                        <p className="text-sm font-semibold text-purple-900">{formatCurrency(cart.decorationAmount)}</p>
                                    </div>
                                </div>
                            )}

                            {/* Additional Items */}
                            {cart.additionalItems && cart.additionalItems.length > 0 && (
                                <div className="mb-4 pb-4 border-b border-neutral-200">
                                    <h4 className="text-sm font-semibold text-neutral-900 mb-3">Additional Items</h4>
                                    {cart.additionalItems.map((item) => (
                                        <div key={item.foodId} className="flex items-center justify-between mb-2 bg-neutral-50 rounded-lg p-2">
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium text-neutral-900 truncate">{item.foodName}</p>
                                                <p className="text-xs text-neutral-500">
                                                    {formatCurrency(item.price)} × {item.quantity}
                                                </p>
                                            </div>
                                            <div className="flex items-center gap-2 ml-2">
                                                <p className="text-sm font-semibold text-neutral-900">
                                                    {formatCurrency(item.price * item.quantity * cart.guestCount)}
                                                </p>
                                                <button
                                                    onClick={() => removeAdditionalItem(item.foodId)}
                                                    className="p-1 hover:bg-red-100 rounded text-red-600 transition-colors"
                                                >
                                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                                    </svg>
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}

                            {/* Compact Price Breakdown */}
                            <div className="bg-neutral-50 rounded-lg p-4">
                                <h4 className="text-sm font-semibold text-neutral-900 mb-3">Price Summary</h4>
                                <div className="space-y-2 text-sm">
                                    <div className="flex justify-between">
                                        <span className="text-neutral-600">Package ({cart.guestCount} guests)</span>
                                        <span className="font-medium text-neutral-900">{formatCurrency(cart.baseAmount)}</span>
                                    </div>
                                    {cart.additionalItemsTotal > 0 && (
                                        <div className="flex justify-between">
                                            <span className="text-neutral-600">Additional Items</span>
                                            <span className="font-medium text-neutral-900">{formatCurrency(cart.additionalItemsTotal)}</span>
                                        </div>
                                    )}
                                    {cart.decorationAmount > 0 && (
                                        <div className="flex justify-between">
                                            <span className="text-neutral-600">Decoration</span>
                                            <span className="font-medium text-neutral-900">{formatCurrency(cart.decorationAmount)}</span>
                                        </div>
                                    )}
                                    <div className="flex justify-between">
                                        <span className="text-neutral-600">GST (18%)</span>
                                        <span className="font-medium text-neutral-900">{formatCurrency(cart.taxAmount)}</span>
                                    </div>
                                    <div className="flex justify-between pt-2 border-t border-neutral-300">
                                        <span className="font-bold text-neutral-900">Total Amount</span>
                                        <span className="font-bold text-orange-600 text-lg">{formatCurrency(cart.totalAmount)}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Clear Cart Button */}
                            <button
                                onClick={() => setShowClearConfirm(true)}
                                className="w-full text-center text-sm text-red-600 hover:text-red-700 hover:bg-red-50 font-medium py-3 rounded-lg transition-colors mt-4"
                            >
                                Clear Cart
                            </button>
                        </div>

                        {/* Footer - Action Buttons */}
                        <div className="px-6 py-4 border-t border-neutral-200 bg-white space-y-2">
                            {/* View Full Cart Button */}
                            <button
                                onClick={handleViewFullCart}
                                className="w-full py-3 text-base font-semibold border-2 border-orange-500 text-orange-600 rounded-lg hover:bg-orange-50 transition-colors flex items-center justify-center gap-2"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                                </svg>
                                View Full Cart
                            </button>

                            {/* Checkout Button */}
                            <button
                                onClick={handleProceedToCheckout}
                                className="w-full btn-primary py-3 text-base font-semibold"
                            >
                                Proceed to Checkout
                            </button>
                        </div>
                    </>
                )}
            </div>

            {/* Clear Cart Confirmation Modal */}
            {showClearConfirm && (
                <div className="fixed inset-0 bg-black bg-opacity-50 z-[60] flex items-center justify-center p-4">
                    <div className="bg-white rounded-xl shadow-2xl max-w-sm w-full p-6 animate-fade-in">
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
