/**
 * FloatingCartButton Component
 *
 * Swiggy/Zomato style floating cart button
 * Visible only when user has items in cart
 * Shows cart count and total amount
 */

import React from 'react';
import { useLocation } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';

export default function FloatingCartButton() {
    const { cart, toggleCart, getCartItemCount } = useCart();
    const { isAuthenticated } = useAuth();
    const location = useLocation();
    const cartCount = getCartItemCount();

    // Hide on catering detail page — that page has its own fixed bottom bar
    if (location.pathname.match(/\/caterings\/\d+/)) return null;

    // Only show when authenticated and cart has items
    if (!isAuthenticated || !cart || cartCount === 0) {
        return null;
    }

    return (
        <button
            onClick={toggleCart}
            className="fixed bottom-6 right-6 z-[999] bg-gradient-to-r from-rose-500 to-rose-600 hover:from-rose-600 hover:to-rose-700 text-white px-6 py-4 rounded-full shadow-2xl hover:shadow-3xl transition-all duration-300 transform hover:scale-105 active:scale-95 flex items-center gap-3 animate-slideUp"
            aria-label="View Cart"
        >
            {/* Cart Icon with Badge */}
            <div className="relative">
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
                        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                </svg>
                {cartCount > 0 && (
                    <span className="absolute -top-2 -right-2 bg-white text-rose-600 text-xs font-bold rounded-full h-5 w-5 flex items-center justify-center shadow-md">
                        {cartCount}
                    </span>
                )}
            </div>

            {/* Cart Info */}
            <div className="flex flex-col items-start">
                <span className="text-xs font-medium opacity-90">View Cart</span>
                <span className="text-sm font-bold">
                    ₹{cart.totalAmount.toLocaleString()}
                </span>
            </div>

            {/* Arrow Icon */}
            <svg
                className="w-5 h-5 ml-1"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
            >
                <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 5l7 7-7 7"
                />
            </svg>
        </button>
    );
}
