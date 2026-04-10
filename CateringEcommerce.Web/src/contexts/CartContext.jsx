/*
========================================
File: src/contexts/CartContext.jsx
========================================
Manages user cart state for catering services.
Cart is event-based - only one caterer allowed per cart.
*/
import React, { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';
import { calculateCartTotals, normalizeDecorationOrNull, normalizeDecorations } from '../utils/cartPricing';

const CartContext = createContext(null);

const buildCartState = (cartData, existingCart = null) => {
    const guestCount = cartData.guestCount ?? existingCart?.guestCount ?? 50;
    const additionalItems = cartData.additionalItems ?? existingCart?.additionalItems ?? [];
    const primaryDecoration = normalizeDecorationOrNull(
        cartData.decorationId != null
            ? {
                decorationId: cartData.decorationId,
                decorationName: cartData.decorationName,
                decorationPrice: cartData.decorationPrice,
            }
            : existingCart?.decorationId != null
                ? {
                    decorationId: existingCart.decorationId,
                    decorationName: existingCart.decorationName,
                    decorationPrice: existingCart.decorationPrice,
                }
                : null
    );
    const standaloneDecorations = normalizeDecorations(
        cartData.standaloneDecorations ?? cartData.decorations ?? existingCart?.standaloneDecorations ?? existingCart?.decorations
    );

    const totals = calculateCartTotals({
        packagePrice: cartData.packagePrice ?? existingCart?.packagePrice ?? 0,
        guestCount,
        additionalItems,
        primaryDecoration,
        standaloneDecorations,
    });

    return {
        ...existingCart,
        ...cartData,
        guestCount: totals.guestCount,
        additionalItems: totals.additionalItems,
        standaloneDecorations: totals.standaloneDecorations,
        decorations: totals.standaloneDecorations,
        decorationIds: totals.standaloneDecorations.map(item => item.decorationId),
        decorationId: totals.primaryDecoration?.decorationId ?? null,
        decorationName: totals.primaryDecoration?.name ?? null,
        decorationPrice: totals.primaryDecorationTotal,
        baseAmount: totals.packageTotal,
        additionalItemsTotal: totals.additionalItemsTotal,
        decorationAmount: totals.decorationAmount,
        standaloneDecorationAmount: totals.standaloneDecorationAmount,
        subtotal: totals.subtotal,
        taxAmount: totals.taxAmount,
        totalAmount: totals.totalAmount,
        updatedAt: new Date().toISOString()
    };
};

export const CartProvider = ({ children }) => {
    const { user, isAuthenticated } = useAuth();
    const [cart, setCart] = useState(null);
    const [isCartOpen, setIsCartOpen] = useState(false);
    const isLoading = false;

    // Load cart from localStorage on mount
    useEffect(() => {
        if (isAuthenticated && user) {
            const savedCart = localStorage.getItem(`cart_${user.pkid}`);
            if (savedCart) {
                try {
                    setCart(buildCartState(JSON.parse(savedCart)));
                } catch (error) {
                    console.error('Error loading cart:', error);
                    localStorage.removeItem(`cart_${user.pkid}`);
                }
            }
        }
    }, [isAuthenticated, user]);

    // Save cart to localStorage whenever it changes
    useEffect(() => {
        if (isAuthenticated && user && cart) {
            localStorage.setItem(`cart_${user.pkid}`, JSON.stringify(cart));
        }
    }, [cart, isAuthenticated, user]);

    /**
     * Add caterer to cart with package and event details
     * Only one caterer allowed per cart - will replace existing cart if different caterer
     * @param {Object} cateringData - The catering data to add
     * @param {boolean} force - Force add even if different caterer exists
     * @returns {Object} - { success: boolean, needsConfirmation?: boolean, message?: string }
     */
    const addToCart = (cateringData, force = false) => {
        const {
            cateringId,
            cateringName,
            cateringLogo,
            packageId,
            packageName,
            packagePrice,
            guestCount: rawGuestCount,
            eventDate = null,
            eventType = null,
            eventLocation = null,
            decorationId = null,
            decorationName = null,
            decorationPrice = 0,
            additionalItems = [],
            packageSelections = null,
            sampleTasteSelections = null
        } = cateringData;

        // null bypasses destructuring defaults — coerce null/undefined to 50
        const guestCount = rawGuestCount ?? 50;

        // Check if trying to add different caterer
        if (cart && cart.cateringId !== cateringId && !force) {
            // Different caterer - return confirmation needed
            return {
                success: false,
                needsConfirmation: true,
                message: `You already have ${cart.cateringName} in your cart. Do you want to replace it with ${cateringName}?`,
                currentCaterer: cart.cateringName,
                newCaterer: cateringName
            };
        }

        const newCart = buildCartState({
            ...cateringData,
            cateringId,
            cateringName,
            cateringLogo
        });

        setCart(newCart);
        setIsCartOpen(true);
        return { success: true };
    };

    /**
     * Update cart details (guest count, event details, etc.)
     */
    const updateCart = (updates) => {
        if (!cart) return;

        setCart(buildCartState(updates, cart));
    };

    /**
     * Add or update additional food item in cart
     */
    const addAdditionalItem = (foodItem) => {
        if (!cart) return;

        const existingItems = cart.additionalItems || [];
        const existingIndex = existingItems.findIndex(item => item.foodId === foodItem.foodId);

        let updatedItems;
        if (existingIndex >= 0) {
            // Update existing item quantity
            updatedItems = [...existingItems];
            updatedItems[existingIndex] = {
                ...updatedItems[existingIndex],
                quantity: updatedItems[existingIndex].quantity + (foodItem.quantity || 1)
            };
        } else {
            // Add new item
            updatedItems = [...existingItems, foodItem];
        }

        updateCart({ additionalItems: updatedItems });
    };

    /**
     * Remove additional food item from cart
     */
    const removeAdditionalItem = (foodId) => {
        if (!cart) return;

        const updatedItems = (cart.additionalItems || []).filter(item => item.foodId !== foodId);
        updateCart({ additionalItems: updatedItems });
    };

    /**
     * Clear the entire cart
     */
    const clearCart = () => {
        setCart(null);
        if (isAuthenticated && user) {
            localStorage.removeItem(`cart_${user.pkid}`);
        }
    };

    /**
     * Get cart item count (always 1 for event-based cart if cart exists)
     */
    const getCartItemCount = () => {
        return cart ? 1 : 0;
    };

    /**
     * Toggle cart drawer open/close
     */
    const toggleCart = () => {
        setIsCartOpen(!isCartOpen);
    };

    const value = {
        cart,
        isCartOpen,
        isLoading,
        addToCart,
        updateCart,
        addAdditionalItem,
        removeAdditionalItem,
        clearCart,
        getCartItemCount,
        toggleCart,
        setIsCartOpen
    };

    return (
        <CartContext.Provider value={value}>
            {children}
        </CartContext.Provider>
    );
};

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error('useCart must be used within a CartProvider');
    }
    return context;
};
