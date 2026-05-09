/*
========================================
File: src/contexts/AuthContext.jsx (UPDATED)
========================================
Cookie-based auth — JWT is stored in httpOnly cookie by the backend.
No token is ever accessible to JavaScript.
*/
import React, { createContext, useState, useContext, useEffect } from 'react';
import { fetchApi } from '../services/apiUtils'

const API_BASE = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '') || '';

// 1. Create the context
const AuthContext = createContext(null);

// 2. Create the provider component
export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    // On mount, check if httpOnly cookie is still valid via /me endpoint
    useEffect(() => {
        const checkAuth = async () => {
            try {
                // First try to restore from localStorage (non-sensitive profile data)
                const storedUser = localStorage.getItem('enyvora_user');
                if (storedUser) {
                    setUser(JSON.parse(storedUser));
                }

                // Validate session with backend (cookie sent automatically)
                if (storedUser) {
                    const res = await fetchApi('/User/Auth/me')
                    if (!res.result) {
                        // Cookie expired or invalid — clear state
                        localStorage.removeItem('enyvora_user');
                        setUser(null);
                    }
                }
            } catch {
                // Network error — keep stored user for offline graceful degradation
            } finally {
                setIsLoading(false);
            }
        };

        checkAuth();
    }, []);

    const login = (userData) => {
        // Token is already set as httpOnly cookie by backend — only store profile data
        const {...profileData } = userData;
        localStorage.setItem('enyvora_user', JSON.stringify(profileData));
        setUser(profileData);
    };

    const logout = async () => {
        try {
            await fetch(`${API_BASE}/api/User/Auth/logout`, {
                method: 'POST',
                credentials: 'include',
            });
        } catch {
            // Proceed with local cleanup even if server call fails
        }
        setUser(null);
        localStorage.removeItem('enyvora_user');
    };

    const updateUserProfileInContext = (updatedData) => {
        setUser(prevUser => {
            const newUser = { ...prevUser, ...updatedData };
            localStorage.setItem('enyvora_user', JSON.stringify(newUser));
            return newUser;
        });
    };

    const value = {
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        updateUserProfileInContext,
    };

    // P1 FIX: Don't return null during loading - this causes blank flash on public pages
    // Protected routes can check isLoading themselves if needed
    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

// 3. Create the custom hook
export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};
