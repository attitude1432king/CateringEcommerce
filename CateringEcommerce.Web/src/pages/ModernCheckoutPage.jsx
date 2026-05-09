import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import { useAppSettings } from '../contexts/AppSettingsContext';
import { createOrder } from '../services/orderApi';
import { createRazorpayOrder, verifyRazorpayPayment } from '../services/paymentApi';
import { loadRazorpayScript } from '../utils/razorpayLoader';
import AccountSection from '../components/user/checkout/modern/AccountSection';
import EventDetailsSection from '../components/user/checkout/modern/EventDetailsSection';
import DeliveryTypeSection from '../components/user/checkout/modern/DeliveryTypeSection';
import PaymentSection from '../components/user/checkout/modern/PaymentSection';
import CartSummary from '../components/user/checkout/modern/CartSummary';
import OrderConfirmationModal from '../components/user/OrderConfirmationModal';
import { validateCheckoutData } from '../utils/checkoutValidator';

/**
 * Modern Checkout Page - Two Column Layout
 * Left: Checkout Steps (Vertical Timeline)
 * Right: Cart Summary (Sticky)
 */
const ModernCheckoutPage = () => {
    const navigate = useNavigate();
    const { cart, clearCart, updateCart } = useCart();
    const { user, isAuthenticated } = useAuth();
    const { getInt } = useAppSettings();

    const [currentStep, setCurrentStep] = useState(1);
    const [completedSteps, setCompletedSteps] = useState([]);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [showConfirmation, setShowConfirmation] = useState(false);
    const [createdOrder, setCreatedOrder] = useState(null);
    const [errors, setErrors] = useState({});

    const buildPackageSelectionPayload = () => {
        if (!cart?.packageSelections && !cart?.sampleTasteSelections) {
            return null;
        }

        return {
            ...(cart.packageSelections || {}),
            sampleTasteSelections: cart.sampleTasteSelections || [],
            sampleTasteMeta: cart.sampleTasteSelections?.length
                ? {
                    source: 'package',
                    status: 'SAMPLE_REQUESTED',
                    selectedItemCount: cart.sampleTasteSelections.reduce(
                        (total, category) => total + (category?.selectedItems?.length || 0),
                        0
                    )
                }
                : null
        };
    };

    const buildIndividualSamplePayload = (item) => {
        const matchingCategory = (cart.sampleTasteSelections || []).find(category =>
            (category?.selectedItems || []).some(selectedItem => selectedItem.foodItemId === item.foodId)
        );

        if (!matchingCategory) {
            return null;
        }

        const selectedItem = matchingCategory.selectedItems.find(sampleItem => sampleItem.foodItemId === item.foodId);

        return {
            sampleTasteSelections: [{
                categoryId: matchingCategory.categoryId,
                categoryName: matchingCategory.categoryName,
                selectedItems: selectedItem ? [selectedItem] : []
            }],
            sampleTasteMeta: {
                source: 'individual',
                status: 'SAMPLE_REQUESTED'
            }
        };
    };

    const [checkoutData, setCheckoutData] = useState({
        // Account (Step 1)
        isGuest: false,
        guestEmail: '',
        guestPhone: '',

        // Event Details (Step 2)
        eventType: '',
        eventDate: cart?.eventDate || null,
        eventTime: null,
        guestCount: cart?.guestCount || 50,
        eventLocation: '',
        eventAddress: {
            street: '',
            city: '',
            state: '',
            pincode: '',
            landmark: ''
        },
        specialInstructions: '',

        // Delivery Type (Step 3)
        deliveryType: 'event', // 'sample' or 'event'
        scheduledDispatchTime: null,

        // Payment (Step 4)
        paymentMethod: 'online', // 'online' | 'split' | 'cod'
        termsAccepted: false
    });

    // Redirect if cart is empty
    useEffect(() => {
        if (!cart || !cart.cateringId) {
            navigate('/');
        }
    }, [cart, navigate]);

    // Auto-advance to step 2 if already authenticated
    useEffect(() => {
        if (isAuthenticated && currentStep === 1) {
            markStepComplete(1);
            setCurrentStep(2);
        }
    }, [isAuthenticated]);

    useEffect(() => {
        if (cart?.eventDate) {
            setCheckoutData(prev => ({
                ...prev,
                eventDate: prev.eventDate || cart.eventDate
            }));
        }
    }, [cart?.eventDate]);

    const updateCheckoutData = (field, value) => {
        setCheckoutData(prev => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors(prev => {
                const next = { ...prev };
                delete next[field];
                return next;
            });
        }
    };

    const markStepComplete = (step) => {
        if (!completedSteps.includes(step)) {
            setCompletedSteps(prev => [...prev, step]);
        }
    };

    const validateStep = (step) => {
        const validation = validateCheckoutData(checkoutData, cart, step, {
            minAdvancePaymentPercent: getInt('BUSINESS.MIN_ADVANCE_PAYMENT_PERCENT', 40),
            minAdvanceBookingDays: getInt('BUSINESS.MIN_ADVANCE_BOOKING_DAYS', 5),
        });
        if (!validation.isValid) {
            setErrors(validation.errors);
            return false;
        }
        setErrors({});
        return true;
    };

    const handleStepComplete = (step) => {
        if (validateStep(step)) {
            markStepComplete(step);
            if (step < 4) {
                setCurrentStep(step + 1);
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        }
    };

    // ── Razorpay payment flow ────────────────────────────────────────────────

    const initiateRazorpayPayment = async (order, stageType) => {
        try {
            await loadRazorpayScript();

            const total    = order.totalAmount || cart.totalAmount || 0;
            const amount   = stageType === 'PreBooking' ? Math.round(total * 0.40) : total;
            const receipt  = `rcpt_${order.orderId}`.slice(0, 40);
            const userId   = user?.userId || user?.id || 0;

            const rzpRes = await createRazorpayOrder({
                Amount:    amount,
                Receipt:   receipt,
                OrderId:   order.orderId,
                UserId:    userId,
                StageType: stageType,
            });

            if (!rzpRes?.result) {
                setErrors({ submit: rzpRes?.message || 'Could not initiate payment. Please try again.' });
                setIsSubmitting(false);
                return;
            }

            const rzpData = rzpRes.data;

            const options = {
                key:         rzpData.key,
                amount:      rzpData.amount,          // in paise (set by server)
                currency:    rzpData.currency || 'INR',
                name:        cart.cateringName || 'CateringEcommerce',
                description: `Order #${order.orderNumber}`,
                order_id:    rzpData.razorpayOrderId,

                handler: async (paymentResponse) => {
                    try {
                        const verifyRes = await verifyRazorpayPayment({
                            RazorpayOrderId:    paymentResponse.razorpay_order_id,
                            RazorpayPaymentId:  paymentResponse.razorpay_payment_id,
                            RazorpaySignature:  paymentResponse.razorpay_signature,
                            OrderId:            order.orderId,
                            StageType:          stageType,
                        });

                        if (verifyRes?.result) {
                            clearCart();
                            setCreatedOrder(order);
                            setShowConfirmation(true);
                        } else {
                            setErrors({
                                submit: verifyRes?.message ||
                                    'Payment verification failed. Please contact support with your Payment ID.'
                            });
                        }
                    } catch (err) {
                        console.error('Payment verification error:', err);
                        setErrors({
                            submit: 'Payment verification error. Contact support with your Payment ID: ' +
                                    paymentResponse.razorpay_payment_id
                        });
                    } finally {
                        setIsSubmitting(false);
                    }
                },

                prefill: {
                    name:    checkoutData.isGuest ? 'Guest' : (user?.name || ''),
                    email:   checkoutData.isGuest ? checkoutData.guestEmail  : (user?.email || ''),
                    contact: checkoutData.isGuest ? checkoutData.guestPhone  : (user?.phone || ''),
                },

                theme: { color: '#e11d48' },

                modal: {
                    ondismiss: () => {
                        setErrors({
                            submit: 'Payment cancelled. Your order has been saved — you can retry payment from your Orders page.'
                        });
                        setIsSubmitting(false);
                    }
                }
            };

            const rzp = new window.Razorpay(options);
            rzp.open();

        } catch (err) {
            console.error('Razorpay error:', err);
            setErrors({ submit: 'Failed to load payment gateway. Please try again.' });
            setIsSubmitting(false);
        }
    };

    // ── Order submission ─────────────────────────────────────────────────────

    const handleSubmitOrder = async () => {
        if (!validateStep(4)) return;

        if (!checkoutData.termsAccepted) {
            setErrors({ termsAccepted: 'Please accept the terms and conditions.' });
            return;
        }

        setIsSubmitting(true);

        try {
            // ── Build order items ────────────────────────────────────────────
            const orderItems = [];

            if (cart.packageId) {
                const packageSelectionsPayload = buildPackageSelectionPayload();
                orderItems.push({
                    ItemType:          'Package',
                    ItemId:            cart.packageId,
                    ItemName:          cart.packageName,
                    Quantity:          1,
                    UnitPrice:         cart.packagePrice,
                    TotalPrice:        cart.packagePrice * checkoutData.guestCount,
                    PackageSelections: packageSelectionsPayload
                        ? JSON.stringify(packageSelectionsPayload)
                        : null,
                });
            }

            if (cart.decorationId) {
                orderItems.push({
                    ItemType:          'Decoration',
                    ItemId:            cart.decorationId,
                    ItemName:          cart.decorationName || 'Package Decoration',
                    Quantity:          1,
                    UnitPrice:         cart.decorationPrice || 0,
                    TotalPrice:        cart.decorationPrice || 0,
                    PackageSelections: null,
                });
            }

            ((cart.standaloneDecorations || cart.decorations || [])).forEach((decoration) => {
                orderItems.push({
                    ItemType:          'Decoration',
                    ItemId:            decoration.decorationId,
                    ItemName:          decoration.name,
                    Quantity:          1,
                    UnitPrice:         decoration.price,
                    TotalPrice:        decoration.price,
                    PackageSelections: null,
                });
            });

            (cart.additionalItems || []).forEach(item => {
                const individualSamplePayload = buildIndividualSamplePayload(item);
                orderItems.push({
                    ItemType:          'FoodItem',
                    ItemId:            item.foodId,
                    ItemName:          item.foodName || item.name,
                    Quantity:          item.quantity || 1,
                    UnitPrice:         item.price,
                    TotalPrice:        item.price * (item.quantity || 1),
                    PackageSelections: individualSamplePayload
                        ? JSON.stringify(individualSamplePayload)
                        : null,
                });
            });

            // ── Map payment method to backend values ─────────────────────────
            const method      = checkoutData.paymentMethod; // 'online' | 'split' | 'cod'
            const isSplit     = method === 'split';
            const isCOD       = method === 'cod';
            const total       = cart.totalAmount || 0;

            const backendPaymentMethod = isCOD ? 'COD' : 'Online';
            const enableSplitPayment   = isSplit;
            const preBookingAmount     = isSplit ? Math.round(total * 0.40) : undefined;
            const postEventAmount      = isSplit ? Math.round(total * 0.60) : undefined;

            // ── Build full event location string ─────────────────────────────
            const addr = checkoutData.eventAddress;
            const eventLocation = [addr.street, addr.city, addr.state, addr.pincode]
                .filter(Boolean).join(', ');

            // ── Contact info: authenticated user or guest ─────────────────────
            const contactPerson = checkoutData.isGuest ? 'Guest' : (user?.name || 'Guest');
            const contactPhone  = checkoutData.isGuest ? checkoutData.guestPhone  : user?.phone;
            const contactEmail = checkoutData.isGuest ? checkoutData.guestEmail : user?.email;


            if (!contactPhone || !contactEmail) {
                setErrors({
                    submit: 'Contact phone and email are required. Please update your profile for checkout.'
                });
                setIsSubmitting(false);
                return;
            }

            const orderData = {
                CateringId:          cart.cateringId,
                EventDate:           checkoutData.eventDate,
                EventTime:           checkoutData.eventTime,
                EventType:           checkoutData.eventType,
                EventLocation:       eventLocation,
                GuestCount:          checkoutData.guestCount,
                SpecialInstructions: checkoutData.specialInstructions || null,
                DeliveryAddress:     eventLocation,
                ContactPerson:       contactPerson,
                ContactPhone:        contactPhone,
                ContactEmail:        contactEmail,
                BaseAmount:          cart.subtotal || cart.baseAmount || 0,
                TaxAmount:           cart.taxAmount    || 0,
                DeliveryCharges:     0,
                DiscountAmount:      cart.discountAmount || 0,
                TotalAmount:         total,
                PaymentMethod:       backendPaymentMethod,
                EnableSplitPayment:  enableSplitPayment,
                DecorationId:        cart.decorationId || null,
                DecorationIds:       (cart.standaloneDecorations || cart.decorations || []).map(item => item.decorationId),
                ...(isSplit && { PreBookingAmount: preBookingAmount, PostEventAmount: postEventAmount }),
                OrderItems:          orderItems,
            };

            const response = await createOrder(orderData);

            if (!response?.result) {
                setErrors({ submit: response?.message || 'Failed to create order. Please try again.' });
                setIsSubmitting(false);
                return;
            }

            const order = response.data;
            setCreatedOrder(order);

            if (isCOD) {
                // COD: no payment gateway needed — show confirmation immediately
                clearCart();
                setShowConfirmation(true);
                setIsSubmitting(false);
            } else {
                // Online / Split: initiate Razorpay (manages isSubmitting internally)
                const stageType = isSplit ? 'PreBooking' : 'Full';
                await initiateRazorpayPayment(order, stageType);
            }

        } catch (error) {
            console.error('Error submitting order:', error);
            setErrors({ submit: 'An unexpected error occurred. Please try again.' });
            setIsSubmitting(false);
        }
    };

    if (!cart) return null;

    return (
        <div className="min-h-screen bg-neutral-50">
            {/* Header */}
            <div className="bg-white border-b sticky top-0 z-40 shadow-sm">
                <div className="max-w-7xl mx-auto px-4 py-4">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                            <button
                                onClick={() => navigate(-1)}
                                className="text-neutral-600 hover:text-neutral-900 transition-colors"
                            >
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                                </svg>
                            </button>
                            <div>
                                <h1 className="text-2xl font-bold text-neutral-900">Checkout</h1>
                                <p className="text-sm text-neutral-600">Complete your catering order</p>
                            </div>
                        </div>
                        <div className="hidden md:flex items-center gap-2 text-sm">
                            <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                            </svg>
                            <span className="text-neutral-700">Secure Checkout</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Main Content - Two Column Layout */}
            <div className="max-w-7xl mx-auto px-4 py-8">
                <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">

                    {/* LEFT COLUMN - Checkout Steps */}
                    <div className="lg:col-span-7 space-y-4">
                        <AccountSection
                            stepNumber={1}
                            isActive={currentStep === 1}
                            isCompleted={completedSteps.includes(1)}
                            checkoutData={checkoutData}
                            updateCheckoutData={updateCheckoutData}
                            errors={errors}
                            onComplete={() => handleStepComplete(1)}
                            onEdit={() => setCurrentStep(1)}
                        />

                        <EventDetailsSection
                            stepNumber={2}
                            isActive={currentStep === 2}
                            isCompleted={completedSteps.includes(2)}
                            checkoutData={checkoutData}
                            updateCheckoutData={updateCheckoutData}
                            updateCart={updateCart}
                            errors={errors}
                            onComplete={() => handleStepComplete(2)}
                            onEdit={() => setCurrentStep(2)}
                            cart={cart}
                        />

                        <DeliveryTypeSection
                            stepNumber={3}
                            isActive={currentStep === 3}
                            isCompleted={completedSteps.includes(3)}
                            checkoutData={checkoutData}
                            updateCheckoutData={updateCheckoutData}
                            errors={errors}
                            onComplete={() => handleStepComplete(3)}
                            onEdit={() => setCurrentStep(3)}
                        />

                        <PaymentSection
                            stepNumber={4}
                            isActive={currentStep === 4}
                            isCompleted={completedSteps.includes(4)}
                            checkoutData={checkoutData}
                            updateCheckoutData={updateCheckoutData}
                            errors={errors}
                            cart={cart}
                            onSubmit={handleSubmitOrder}
                            isSubmitting={isSubmitting}
                        />
                    </div>

                    {/* RIGHT COLUMN - Cart Summary (Sticky) */}
                    <div className="lg:col-span-5">
                        <div className="sticky top-24">
                            <CartSummary
                                cart={cart}
                                checkoutData={checkoutData}
                                canPlaceOrder={completedSteps.length >= 3 && currentStep === 4}
                                onPlaceOrder={handleSubmitOrder}
                                isSubmitting={isSubmitting}
                            />
                        </div>
                    </div>
                </div>
            </div>

            {/* Order Confirmation Modal */}
            {showConfirmation && createdOrder && (
                <OrderConfirmationModal
                    order={createdOrder}
                    onClose={() => {
                        setShowConfirmation(false);
                        navigate('/my-orders');
                    }}
                />
            )}
        </div>
    );
};

export default ModernCheckoutPage;
