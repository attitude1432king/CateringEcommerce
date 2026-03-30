import React from 'react';
import { formatEventDate, formatEventTime, getEventTypeDisplay } from '../../../../utils/checkoutValidator';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';


/**
 * Cart Summary Component - Sticky Right Column
 * Shows caterer info, cart items, pricing breakdown
 */
const CartSummary = ({ cart, checkoutData, canPlaceOrder, onPlaceOrder, isSubmitting }) => {
    if (!cart) return null;

    const calculateTotals = () => {
        // Use the guest count from checkoutData if available, otherwise use cart's guest count
        const guestCount = checkoutData?.guestCount || cart.guestCount || 50;

        const packageTotal = (cart.packagePrice || 0) * guestCount;
        const decorationTotal = cart.decorationPrice || 0;
        const additionalTotal = cart.additionalItems?.reduce(
            (sum, item) => sum + (item.price * (item.quantity || 1) * guestCount),
            0
        ) || 0;

        const subtotal = packageTotal + decorationTotal + additionalTotal;

        // Prefer server-computed tax; fall back to 18% local estimate
        const gst = cart.taxAmount != null ? Number(cart.taxAmount) : subtotal * 0.18;

        // Prefer server-computed total; fall back to local sum
        const total = cart.totalAmount != null ? Number(cart.totalAmount) : subtotal + gst;

        return { packageTotal, decorationTotal, additionalTotal, subtotal, gst, platformFee: 0, total };
    };

    const totals = calculateTotals();

    return (
        <div className="bg-white rounded-xl shadow-lg overflow-hidden">
            {/* Caterer Header */}
            <div className="bg-gradient-to-r from-orange-500 to-red-500 p-4 text-white">
                <div className="flex items-center gap-3">
                    {cart.cateringLogo && (
                        <img
                            src={`${API_BASE_URL}${cart.cateringLogo}`}
                            alt={cart.cateringName}
                            className="w-14 h-14 rounded-lg object-cover border-2 border-white shadow-md"
                        />
                    )}
                    <div className="flex-1">
                        <h3 className="font-bold text-lg">{cart.cateringName}</h3>
                        <div className="flex items-center gap-2 mt-1">
                            <span className="bg-white bg-opacity-20 text-xs px-2 py-0.5 rounded-full">
                                ✓ Verified
                            </span>
                            {(cart.catererRating ?? cart.rating) != null && (
                                <span className="bg-white bg-opacity-20 text-xs px-2 py-0.5 rounded-full">
                                    ⭐ {Number(cart.catererRating ?? cart.rating).toFixed(1)}
                                </span>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Cart Items */}
            <div className="p-4 border-b">
                <h4 className="font-semibold text-gray-900 mb-3">Order Summary</h4>

                {/* Package */}
                {cart.packageName && (
                    <div className="mb-4 pb-4 border-b">
                        <div className="flex justify-between items-start mb-2">
                            <div className="flex-1">
                                <div className="font-medium text-gray-900">{cart.packageName}</div>
                                <div className="text-xs text-gray-500 mt-1">
                                    ₹{cart.packagePrice} × {totals.guestCount} guests
                                </div>
                                {cart.packageCategory && (
                                    <span className="inline-block mt-1 text-xs bg-orange-50 text-orange-700 px-2 py-0.5 rounded">
                                        {cart.packageCategory}
                                    </span>
                                )}
                            </div>
                            <div className="font-semibold text-gray-900">
                                ₹{totals.packageTotal.toLocaleString('en-IN')}
                            </div>
                        </div>
                        {cart.packageSelections && (
                            <div className="mt-2 text-xs text-green-600 bg-green-50 px-2 py-1 rounded">
                                ✓ Package customized
                            </div>
                        )}
                    </div>
                )}

                {/* Decoration */}
                {cart.decorationName && (
                    <div className="mb-3 flex justify-between items-center">
                        <div>
                            <div className="font-medium text-gray-900">🎨 {cart.decorationName}</div>
                            <div className="text-xs text-gray-500">Decoration & Setup</div>
                        </div>
                        <div className="font-semibold text-gray-900">
                            ₹{totals.decorationTotal.toLocaleString('en-IN')}
                        </div>
                    </div>
                )}

                {/* Additional Items */}
                {cart.additionalItems?.length > 0 && (
                    <div className="mt-3 pt-3 border-t">
                        <div className="text-xs font-semibold text-gray-700 mb-2">
                            Additional Items
                        </div>
                        {cart.additionalItems.map((item, index) => (
                            <div key={index} className="flex justify-between items-center mb-2 text-sm">
                                <div className="text-gray-700">
                                    {item.foodName || item.name}
                                    <span className="text-xs text-gray-500 ml-1">
                                        (×{item.quantity || 1} × {totals.guestCount})
                                    </span>
                                </div>
                                <div className="text-gray-900">
                                    ₹{(
                                        item.price *
                                        (item.quantity || 1) *
                                        totals.guestCount
                                    ).toLocaleString('en-IN')}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Event Info (if available) */}
            {checkoutData.eventDate && (
                <div className="p-4 bg-orange-50 border-b">
                    <div className="flex items-start gap-2">
                        <svg className="w-5 h-5 text-orange-600 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clipRule="evenodd" />
                        </svg>
                        <div className="flex-1">
                            <div className="text-xs font-medium text-orange-900">Event Details</div>
                            <div className="text-sm font-semibold text-gray-900 mt-1">
                                {formatEventDate(checkoutData.eventDate)}
                            </div>
                            {checkoutData.eventTime && (
                                <div className="text-xs text-gray-700 mt-0.5">
                                    Time: {formatEventTime(checkoutData.eventTime)}
                                </div>
                            )}
                            {checkoutData.eventType && (
                                <div className="text-xs text-gray-700 mt-0.5">
                                    Type: {getEventTypeDisplay(checkoutData.eventType)}
                                </div>
                            )}
                            <div className="text-xs text-gray-700 mt-0.5">
                                Guests: {checkoutData.guestCount || cart.guestCount || 50}
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Bill Details */}
            <div className="p-4">
                <h4 className="font-semibold text-gray-900 mb-3">Bill Details</h4>
                <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                        <span>Item Total</span>
                        <span>
                            ₹{totals.subtotal.toLocaleString('en-IN', {
                                minimumFractionDigits: 2,
                            })}
                        </span>
                    </div>

                    <div className="flex justify-between text-sm">
                        <span>GST &amp; Taxes</span>
                        <span>
                            ₹{totals.gst.toLocaleString('en-IN', {
                                minimumFractionDigits: 2,
                            })}
                        </span>
                    </div>
                </div>

                <div className="border-t border-dashed border-gray-300 my-3"></div>

                <div className="flex justify-between items-center">
                    <span className="font-bold">Total Amount</span>
                    <span className="text-2xl font-bold text-red-600">
                        ₹{totals.total.toLocaleString('en-IN', {
                            minimumFractionDigits: 2,
                        })}
                    </span>
                </div>
            </div>

            {/* Place Order */}
            {canPlaceOrder && (
                <div className="p-4 bg-gray-50 border-t">
                    <button
                        onClick={onPlaceOrder}
                        disabled={isSubmitting}
                        className="w-full bg-orange-500 text-white py-3 rounded-lg font-bold disabled:opacity-50"
                    >
                        {isSubmitting ? 'Processing...' : `Place Order - ₹${totals.total.toLocaleString('en-IN')}`}
                    </button>
                    <div className="text-center mt-2">
                        <span className="text-xs text-gray-500">
                            🔒 Safe and secure payments
                        </span>
                    </div>
                </div>
            )}
        </div>
    );
};

export default CartSummary;
