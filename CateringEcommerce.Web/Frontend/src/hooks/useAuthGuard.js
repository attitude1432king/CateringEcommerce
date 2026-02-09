/**
 * useAuthGuard Hook
 *
 * Provides authentication guard functionality for cart-to-checkout flow
 * Similar to Swiggy/Zomato - shows auth modal instead of redirecting
 */

import { useState, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

export const useAuthGuard = () => {
    const { isAuthenticated, user } = useAuth();
    const [showAuthModal, setShowAuthModal] = useState(false);
    const [pendingAction, setPendingAction] = useState(null);
    const navigate = useNavigate();

    /**
     * Check if user is authenticated before proceeding
     * If not authenticated, show auth modal
     *
     * @param {Function} action - Action to perform after authentication
     * @param {string} redirectPath - Path to redirect after authentication (optional)
     * @returns {boolean} - True if authenticated, false if auth modal shown
     */
    const requireAuth = useCallback((action = null, redirectPath = null) => {
        if (isAuthenticated && user) {
            // User is authenticated, execute action immediately
            if (action && typeof action === 'function') {
                action();
            }
            return true;
        }

        // User not authenticated - show auth modal
        if (redirectPath) {
            // Store redirect path for post-login navigation
            localStorage.setItem('auth_redirect', redirectPath);
        }

        if (action) {
            // Store action to execute after authentication
            setPendingAction(() => action);
        }

        setShowAuthModal(true);
        return false;
    }, [isAuthenticated, user]);

    /**
     * Handle successful authentication
     * Execute pending action and/or navigate to redirect path
     */
    const handleAuthSuccess = useCallback(() => {
        setShowAuthModal(false);

        // Execute pending action
        if (pendingAction) {
            pendingAction();
            setPendingAction(null);
        }

        // Check for stored redirect path
        const redirectPath = localStorage.getItem('auth_redirect');
        if (redirectPath) {
            localStorage.removeItem('auth_redirect');
            navigate(redirectPath);
        }
    }, [pendingAction, navigate]);

    /**
     * Handle auth modal close
     */
    const handleAuthClose = useCallback(() => {
        setShowAuthModal(false);
        setPendingAction(null);
        localStorage.removeItem('auth_redirect');
    }, []);

    /**
     * Manually trigger auth modal
     */
    const triggerAuth = useCallback((action = null) => {
        if (action) {
            setPendingAction(() => action);
        }
        setShowAuthModal(true);
    }, []);

    return {
        isAuthenticated,
        user,
        requireAuth,
        showAuthModal,
        handleAuthSuccess,
        handleAuthClose,
        triggerAuth
    };
};

export default useAuthGuard;
