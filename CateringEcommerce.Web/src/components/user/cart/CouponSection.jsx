import React, { useState, useEffect } from 'react';
import { message } from 'antd';
import { fetchApi } from '../../../services/apiUtils';

/**
 * Coupon Section Component
 * Allows users to browse and apply discount coupons from the caterer
 */
const CouponSection = ({ cateringId, subtotal, appliedCoupon, onApplyCoupon, onRemoveCoupon }) => {
  const [coupons, setCoupons] = useState([]);
  const [loading, setLoading] = useState(false);
  const [showCoupons, setShowCoupons] = useState(false);
  const [couponCode, setCouponCode] = useState('');
  const [applying, setApplying] = useState(false);

  useEffect(() => {
    if (cateringId && showCoupons) {
      fetchAvailableCoupons();
    }
  }, [cateringId, showCoupons]);

  const fetchAvailableCoupons = async () => {
    try {
      setLoading(true);
      // Fetch available coupons for this caterer
      const response = await fetchApi(`/User/Coupons/Available/${cateringId}`, 'GET');

      if (response.success) {
        setCoupons(response.data || []);
      }
    } catch (error) {
      console.error('Error fetching coupons:', error);
    } finally {
      setLoading(false);
    }
  };

  const validateCoupon = (coupon) => {
    const now = new Date();
    const startDate = new Date(coupon.validFrom);
    const endDate = new Date(coupon.validTo);

    // Check if coupon is active
    if (!coupon.isActive) {
      return { valid: false, message: 'This coupon is no longer active' };
    }

    // Check date validity
    if (now < startDate) {
      return { valid: false, message: 'This coupon is not yet valid' };
    }

    if (now > endDate) {
      return { valid: false, message: 'This coupon has expired' };
    }

    // Check minimum order value
    if (coupon.minOrderValue && subtotal < coupon.minOrderValue) {
      return {
        valid: false,
        message: `Minimum order value of ₹${coupon.minOrderValue.toLocaleString('en-IN')} required`
      };
    }

    // Check usage limit
    if (coupon.usageLimit && coupon.usedCount >= coupon.usageLimit) {
      return { valid: false, message: 'This coupon has reached its usage limit' };
    }

    return { valid: true, message: 'Coupon is valid' };
  };

  const calculateDiscount = (coupon) => {
    if (coupon.discountType === 'Percentage') {
      let discount = (subtotal * coupon.discountValue) / 100;
      if (coupon.maxDiscount && discount > coupon.maxDiscount) {
        discount = coupon.maxDiscount;
      }
      return discount;
    } else {
      return Math.min(coupon.discountValue, subtotal);
    }
  };

  const handleApplyCoupon = (coupon) => {
    const validation = validateCoupon(coupon);

    if (!validation.valid) {
      message.error(validation.message);
      return;
    }

    const discount = calculateDiscount(coupon);

    onApplyCoupon({
      ...coupon,
      discountAmount: discount
    });

    message.success(`Coupon applied! You saved ₹${discount.toLocaleString('en-IN')}`);
    setShowCoupons(false);
  };

  const handleApplyCode = async () => {
    if (!couponCode.trim()) {
      message.warning('Please enter a coupon code');
      return;
    }

    setApplying(true);

    try {
      // Validate coupon code with backend
      const response = await fetchApi(`/User/Coupons/Validate`, 'POST', {
        code: couponCode.toUpperCase(),
        cateringId: cateringId,
        orderValue: subtotal
      });

      if (response.success) {
        const coupon = response.data;
        const discount = calculateDiscount(coupon);

        onApplyCoupon({
          ...coupon,
          discountAmount: discount
        });

        message.success(`Coupon applied! You saved ₹${discount.toLocaleString('en-IN')}`);
        setCouponCode('');
      } else {
        message.error(response.message || 'Invalid coupon code');
      }
    } catch (error) {
      message.error('Failed to apply coupon. Please try again.');
    } finally {
      setApplying(false);
    }
  };

  const getCouponBadgeColor = (discountType) => {
    return discountType === 'Percentage'
      ? 'bg-green-100 text-green-700 border-green-200'
      : 'bg-blue-100 text-blue-700 border-blue-200';
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="p-6 border-b bg-gradient-to-r from-green-50 to-teal-50">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <svg className="w-6 h-6 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5 5a3 3 0 015-2.236A3 3 0 0114.83 6H16a2 2 0 110 4h-5V9a1 1 0 10-2 0v1H4a2 2 0 110-4h1.17C5.06 5.687 5 5.35 5 5zm4 1V5a1 1 0 10-1 1h1zm3 0a1 1 0 10-1-1v1h1z" clipRule="evenodd" />
                <path d="M9 11H3v5a2 2 0 002 2h4v-7zM11 18h4a2 2 0 002-2v-5h-6v7z" />
              </svg>
            </div>
            <div>
              <h3 className="font-bold text-neutral-900">Apply Coupon</h3>
              <p className="text-xs text-neutral-600">Save money on your order</p>
            </div>
          </div>
          {appliedCoupon && (
            <span className="inline-flex items-center gap-1 bg-green-100 text-green-700 text-xs font-semibold px-3 py-1 rounded-full">
              <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
              Applied
            </span>
          )}
        </div>
      </div>

      {/* Content */}
      <div className="p-6">
        {/* Applied Coupon Display */}
        {appliedCoupon ? (
          <div className="mb-4 p-4 bg-green-50 border-2 border-green-200 rounded-lg">
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <span className="font-bold text-green-900 text-lg">
                    {appliedCoupon.code || appliedCoupon.couponCode}
                  </span>
                  <span className={`text-xs font-medium px-2 py-0.5 rounded-full border ${getCouponBadgeColor(appliedCoupon.discountType)}`}>
                    {appliedCoupon.discountType === 'Percentage'
                      ? `${appliedCoupon.discountValue}% OFF`
                      : `₹${appliedCoupon.discountValue} OFF`}
                  </span>
                </div>
                <p className="text-sm text-green-700">
                  {appliedCoupon.description || 'Discount applied successfully'}
                </p>
                <p className="text-xs text-green-600 mt-1 font-semibold">
                  You saved ₹{appliedCoupon.discountAmount?.toLocaleString('en-IN')}
                </p>
              </div>
              <button
                onClick={onRemoveCoupon}
                className="ml-4 p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                title="Remove coupon"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>
        ) : (
          <>
            {/* Coupon Code Input */}
            <div className="mb-4">
              <div className="flex gap-2">
                <input
                  type="text"
                  value={couponCode}
                  onChange={(e) => setCouponCode(e.target.value.toUpperCase())}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleApplyCode();
                    }
                  }}
                  placeholder="Enter coupon code"
                  className="flex-1 px-4 py-3 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-green-500 uppercase font-mono text-sm"
                />
                <button
                  onClick={handleApplyCode}
                  disabled={applying || !couponCode.trim()}
                  className="px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium"
                >
                  {applying ? 'Applying...' : 'Apply'}
                </button>
              </div>
            </div>

            {/* Toggle Available Coupons */}
            <button
              onClick={() => setShowCoupons(!showCoupons)}
              className="w-full flex items-center justify-between p-3 bg-gray-50 hover:bg-gray-100 border border-gray-200 rounded-lg transition-colors"
            >
              <span className="text-sm font-medium text-neutral-700">
                {showCoupons ? 'Hide' : 'View'} Available Coupons
              </span>
              <svg
                className={`w-5 h-5 text-neutral-500 transition-transform ${showCoupons ? 'rotate-180' : ''}`}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
              </svg>
            </button>

            {/* Available Coupons List */}
            {showCoupons && (
              <div className="mt-4 space-y-3 max-h-80 overflow-y-auto">
                {loading ? (
                  <div className="text-center py-8">
                    <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
                    <p className="text-sm text-neutral-600 mt-2">Loading coupons...</p>
                  </div>
                ) : coupons.length === 0 ? (
                  <div className="text-center py-8">
                    <svg className="w-16 h-16 text-gray-300 mx-auto mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                    </svg>
                    <p className="text-sm text-neutral-600">No coupons available at the moment</p>
                  </div>
                ) : (
                  coupons.map((coupon) => {
                    const validation = validateCoupon(coupon);
                    const discount = calculateDiscount(coupon);

                    return (
                      <div
                        key={coupon.discountId || coupon.id}
                        className={`p-4 border-2 rounded-lg ${
                          validation.valid
                            ? 'border-green-200 bg-green-50 hover:bg-green-100'
                            : 'border-gray-200 bg-gray-50 opacity-60'
                        } transition-colors`}
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-2">
                              <span className="font-mono font-bold text-neutral-900 text-lg">
                                {coupon.couponCode || coupon.code}
                              </span>
                              <span className={`text-xs font-medium px-2 py-0.5 rounded-full border ${getCouponBadgeColor(coupon.discountType)}`}>
                                {coupon.discountType === 'Percentage'
                                  ? `${coupon.discountValue}% OFF`
                                  : `₹${coupon.discountValue} OFF`}
                              </span>
                            </div>
                            <p className="text-sm text-neutral-700 mb-2">
                              {coupon.description || 'Special discount offer'}
                            </p>
                            <div className="flex flex-wrap gap-2 text-xs text-neutral-600">
                              {coupon.minOrderValue && (
                                <span className="inline-flex items-center gap-1">
                                  <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M4 4a2 2 0 00-2 2v4a2 2 0 002 2V6h10a2 2 0 00-2-2H4zm2 6a2 2 0 012-2h8a2 2 0 012 2v4a2 2 0 01-2 2H8a2 2 0 01-2-2v-4zm6 4a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd" />
                                  </svg>
                                  Min order: ₹{coupon.minOrderValue.toLocaleString('en-IN')}
                                </span>
                              )}
                              {coupon.maxDiscount && coupon.discountType === 'Percentage' && (
                                <span className="inline-flex items-center gap-1">
                                  <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-11a1 1 0 10-2 0v2H7a1 1 0 100 2h2v2a1 1 0 102 0v-2h2a1 1 0 100-2h-2V7z" clipRule="evenodd" />
                                  </svg>
                                  Max: ₹{coupon.maxDiscount.toLocaleString('en-IN')}
                                </span>
                              )}
                              <span className="inline-flex items-center gap-1">
                                <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                  <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clipRule="evenodd" />
                                </svg>
                                Valid till {new Date(coupon.validTo).toLocaleDateString('en-IN')}
                              </span>
                            </div>
                            {validation.valid && (
                              <p className="text-xs text-green-700 font-semibold mt-2">
                                You'll save ₹{discount.toLocaleString('en-IN')}
                              </p>
                            )}
                            {!validation.valid && (
                              <p className="text-xs text-red-600 mt-2">{validation.message}</p>
                            )}
                          </div>
                          <button
                            onClick={() => handleApplyCoupon(coupon)}
                            disabled={!validation.valid}
                            className="ml-4 px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                          >
                            Apply
                          </button>
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default CouponSection;
