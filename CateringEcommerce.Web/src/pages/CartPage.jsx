import React, { useState, useEffect } from 'react';
import { useCart } from '../contexts/CartContext';
import { useNavigate } from 'react-router-dom';
import PackageDetailsCard from '../components/user/cart/PackageDetailsCard';
import GuestCountSelector from '../components/user/cart/GuestCountSelector';
import CouponSection from '../components/user/cart/CouponSection';
import PriceSummaryCard from '../components/user/cart/PriceSummaryCard';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

/**
 * Modern Cart Page - Full Page View
 * Shows comprehensive package details, categories, items, guest count, coupons
 */
const CartPage = () => {
  const { cart, updateCart, clearCart, removeAdditionalItem } = useCart();
  const navigate = useNavigate();
  const [appliedCoupon, setAppliedCoupon] = useState(null);
  const [discountAmount, setDiscountAmount] = useState(0);
  const [showClearConfirm, setShowClearConfirm] = useState(false);

  useEffect(() => {
    // Redirect to home if cart is empty
    if (!cart || !cart.cateringId) {
      navigate('/');
    }
  }, [cart, navigate]);

  const handleGuestCountChange = (newCount) => {
    if (newCount >= 50 && newCount <= 10000) {
      updateCart({ guestCount: newCount });
      // Recalculate discount if coupon applied
      if (appliedCoupon) {
        calculateDiscount(appliedCoupon, newCount);
      }
    }
  };

  const calculateDiscount = (coupon, guestCount = cart?.guestCount) => {
    if (!coupon || !cart) return 0;

    const baseAmount = (cart.packagePrice || 0) * guestCount;
    const additionalTotal = cart.additionalItems?.reduce(
      (sum, item) => sum + (item.price * (item.quantity || 1) * guestCount),
      0
    ) || 0;
    const subtotal = baseAmount + additionalTotal;

    let discount = 0;
    if (coupon.discountType === 'percentage') {
      discount = (subtotal * coupon.discountValue) / 100;
      if (coupon.maxDiscount && discount > coupon.maxDiscount) {
        discount = coupon.maxDiscount;
      }
    } else {
      discount = coupon.discountValue;
    }

    setDiscountAmount(discount);
    updateCart({ discountAmount: discount, appliedCoupon: coupon.code });
    return discount;
  };

  const handleApplyCoupon = (coupon) => {
    setAppliedCoupon(coupon);
    calculateDiscount(coupon);
  };

  const handleRemoveCoupon = () => {
    setAppliedCoupon(null);
    setDiscountAmount(0);
    updateCart({ discountAmount: 0, appliedCoupon: null });
  };

  const handleProceedToCheckout = () => {
    navigate('/checkout'); // P0 FIX: Correct route path
  };

  const handleContinueShopping = () => {
    navigate('/caterings'); // P2 FIX: Correct route path
  };

  const handleClearCartConfirm = () => {
    clearCart();
    setShowClearConfirm(false);
    navigate('/');
  };

  if (!cart) {
    return null;
  }

  return (
    <div className="min-h-screen bg-neutral-50">
      {/* Header */}
      <div className="bg-white border-b shadow-sm sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button onClick={() => navigate(-1)} className="icon-btn shrink-0" aria-label="Go back">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-xl font-bold text-neutral-900">Your Cart</h1>
                <p className="text-sm text-neutral-500">Review your catering order</p>
              </div>
            </div>
            <button
              onClick={handleContinueShopping}
              className="hidden md:flex items-center gap-2 px-4 py-2 text-primary hover:bg-primary/5 rounded-lg transition-colors"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              <span className="font-medium">Add More Items</span>
            </button>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* LEFT COLUMN - Cart Items & Details */}
          <div className="lg:col-span-8 space-y-6">
            {/* Caterer Info Card */}
            <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
              <div className="flex items-start gap-4">
                {cart.cateringLogo && (
                  <img
                    src={`${API_BASE_URL}${cart.cateringLogo}`}
                    alt={cart.cateringName}
                    className="w-20 h-20 rounded-lg object-cover border-2 border-neutral-200 shadow-md"
                  />
                )}
                <div className="flex-1">
                  <h2 className="text-xl font-bold text-neutral-900">{cart.cateringName}</h2>
                  <div className="flex items-center gap-3 mt-2">
                    <span className="inline-flex items-center gap-1 bg-green-50 text-green-700 text-xs font-medium px-2.5 py-1 rounded-full">
                      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                      </svg>
                      Verified Partner
                    </span>
                    <span className="inline-flex items-center gap-1 bg-yellow-50 text-yellow-700 text-xs font-medium px-2.5 py-1 rounded-full">
                      <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                      4.5 Rating
                    </span>
                    <span className="text-xs text-neutral-500">FSSAI Certified</span>
                  </div>
                </div>
                <button
                  onClick={() => setShowClearConfirm(true)}
                  className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  title="Clear cart"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </div>

            {/* Guest Count Selector */}
            <GuestCountSelector
              guestCount={cart.guestCount}
              onGuestCountChange={handleGuestCountChange}
              packageMinimum={cart.minimumGuests || 50}
            />

            {/* Package Details with Categories */}
            <PackageDetailsCard
              packageData={cart}
              guestCount={cart.guestCount}
            />

            {/* Decoration (if selected) */}
            {cart.decorationName && (
              <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
                      <span className="text-2xl">🎨</span>
                    </div>
                    <div>
                      <h3 className="font-semibold text-neutral-900">Decoration Theme</h3>
                      <p className="text-sm text-neutral-600">{cart.decorationName}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-neutral-900">
                      ₹{cart.decorationPrice?.toLocaleString('en-IN')}
                    </p>
                    <p className="text-xs text-neutral-500">One-time setup</p>
                  </div>
                </div>
              </div>
            )}

            {/* Additional Items */}
            {cart.additionalItems && cart.additionalItems.length > 0 && (
              <div className="bg-white rounded-xl shadow-sm border border-neutral-200 p-6">
                <h3 className="text-lg font-bold text-neutral-900 mb-4">Additional Items</h3>
                <div className="space-y-3">
                  {cart.additionalItems.map((item) => (
                    <div
                      key={item.foodId}
                      className="flex items-center justify-between p-4 bg-neutral-50 rounded-lg hover:bg-neutral-100 transition-colors"
                    >
                      <div className="flex-1">
                        <h4 className="font-medium text-neutral-900">{item.foodName || item.name}</h4>
                        <p className="text-sm text-neutral-600 mt-1">
                          ₹{item.price?.toLocaleString('en-IN')} × {item.quantity} × {cart.guestCount} guests
                        </p>
                      </div>
                      <div className="flex items-center gap-4">
                        <p className="text-lg font-bold text-neutral-900">
                          ₹{(item.price * (item.quantity || 1) * cart.guestCount).toLocaleString('en-IN')}
                        </p>
                        <button
                          onClick={() => removeAdditionalItem(item.foodId)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Coupon Section */}
            <CouponSection
              cateringId={cart.cateringId}
              subtotal={cart.baseAmount}
              appliedCoupon={appliedCoupon}
              onApplyCoupon={handleApplyCoupon}
              onRemoveCoupon={handleRemoveCoupon}
            />
          </div>

          {/* RIGHT COLUMN - Price Summary (Sticky) */}
          <div className="lg:col-span-4">
            <div className="sticky top-24">
              <PriceSummaryCard
                cart={cart}
                discountAmount={discountAmount}
                appliedCoupon={appliedCoupon}
                onProceedToCheckout={handleProceedToCheckout}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Clear Cart Confirmation Modal */}
      {showClearConfirm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl max-w-sm w-full p-6 animate-fade-in">
            <div className="text-center mb-6">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
                <svg className="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-neutral-900 mb-2">Clear Cart?</h3>
              <p className="text-sm text-neutral-600">
                Are you sure you want to remove all items from your cart? This action cannot be undone.
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setShowClearConfirm(false)}
                className="flex-1 px-4 py-2.5 bg-neutral-100 text-neutral-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
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
    </div>
  );
};

export default CartPage;
