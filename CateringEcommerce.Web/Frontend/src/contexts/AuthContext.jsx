/*
========================================
File: src/contexts/AuthContext.jsx (UPDATED)
========================================
This file now handles user roles to differentiate between clients and owners.
*/
import React, { createContext, useState, useContext, useEffect } from 'react';

// 1. Create the context
const AuthContext = createContext(null);

// 2. Create the provider component
export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null); // Will hold { pkid, name, role }
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        try {
            const storedUser = localStorage.getItem('feasto_user');
            if (storedUser) {
                setUser(JSON.parse(storedUser));
            }
        } catch (error) {
            console.error("Failed to parse user from localStorage", error);
            localStorage.removeItem('feasto_user');
        } finally {
            setIsLoading(false);
        }
    }, []);


    const login = (userData) => {
        setUser(userData);
        localStorage.setItem('feasto_user', JSON.stringify(userData));
        console.log("User logged in and session saved:", userData);
    };

    const logout = () => {
        setUser(null);
        localStorage.removeItem('feasto_user');
        console.log("User logged out and session cleared.");
    };

    const updateUserProfileInContext = (updatedData) => {
        setUser(prevUser => {
            const newUser = { ...prevUser, ...updatedData };
            localStorage.setItem('feasto_user', JSON.stringify(newUser));
            console.log("User context updated:", newUser);
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

    if (isLoading) {
        return null;
    }

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
