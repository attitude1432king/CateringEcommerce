/*
========================================
File: src/contexts/CartContext.jsx
========================================
Manages user cart state for catering services.
Cart is event-based - only one caterer allowed per cart.
*/
import React, { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';

const CartContext = createContext(null);

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
                    setCart(JSON.parse(savedCart));
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

        // Calculate amounts
        const baseAmount = (packagePrice || 0) * guestCount;
        const additionalItemsTotal = additionalItems.reduce(
            (sum, item) => sum + (item.price * item.quantity * guestCount),
            0
        );
        const decorationAmount = decorationPrice || 0;
        const subtotal = baseAmount + additionalItemsTotal + decorationAmount;
        const taxAmount = subtotal * 0.18; // 18% GST
        const totalAmount = subtotal + taxAmount;

        const newCart = {
            cateringId,
            cateringName,
            cateringLogo,
            packageId,
            packageName,
            packagePrice,
            guestCount,
            eventDate,
            eventType,
            eventLocation,
            decorationId,
            decorationName,
            decorationPrice,
            additionalItems,
            packageSelections,
            sampleTasteSelections,
            baseAmount,
            additionalItemsTotal,
            decorationAmount,
            taxAmount,
            totalAmount,
            updatedAt: new Date().toISOString()
        };

        setCart(newCart);
        setIsCartOpen(true);
        return { success: true };
    };

    /**
     * Update cart details (guest count, event details, etc.)
     */
    const updateCart = (updates) => {
        if (!cart) return;

        const updatedCart = { ...cart, ...updates };

        // Recalculate amounts if guest count or items changed
        if (updates.guestCount || updates.additionalItems) {
            const guestCount = updates.guestCount || cart.guestCount;
            const additionalItems = updates.additionalItems || cart.additionalItems;

            const baseAmount = (cart.packagePrice || 0) * guestCount;
            const additionalItemsTotal = additionalItems.reduce(
                (sum, item) => sum + (item.price * item.quantity * guestCount),
                0
            );
            const subtotal = baseAmount + additionalItemsTotal + (cart.decorationAmount || 0);
            const taxAmount = subtotal * 0.18;
            const totalAmount = subtotal + taxAmount;

            updatedCart.baseAmount = baseAmount;
            updatedCart.additionalItemsTotal = additionalItemsTotal;
            updatedCart.taxAmount = taxAmount;
            updatedCart.totalAmount = totalAmount;
        }

        updatedCart.updatedAt = new Date().toISOString();
        setCart(updatedCart);
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
