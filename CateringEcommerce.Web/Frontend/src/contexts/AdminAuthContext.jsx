import { createContext, useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchApi } from '../services/apiUtils';

const AdminAuthContext = createContext(null);

export const useAdminAuth = () => {
    const context = useContext(AdminAuthContext);
    if (!context) {
        throw new Error('useAdminAuth must be used within AdminAuthProvider');
    }
    return context;
};

export const AdminAuthProvider = ({ children }) => {
    const [admin, setAdmin] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    // SECURITY FIX: Check for stored auth on mount
    // Token now in httpOnly cookie, only check admin data
    useEffect(() => {
        const storedAdmin = localStorage.getItem('admin');

        if (storedAdmin) {
            setAdmin(JSON.parse(storedAdmin));
        }
        setLoading(false);
    }, []);

    const login = async (username, password) => {
        try {
            const payload = {};
            payload.username = username;
            payload.password = password;
            // SECURITY FIX: withCredentials allows cookies to be set/sent
            const result = await fetchApi('/admin/auth/login', 'POST', payload, {
                credentials: 'include'  // Important: allows httpOnly cookie
            });

            if (result.result && result.data) {
                // SECURITY FIX: Token no longer returned (it's in httpOnly cookie)
                // Only store non-sensitive admin data
                const adminData = result.data;

                // Store admin data (NOT token) in localStorage
                localStorage.setItem('admin', JSON.stringify(adminData));

                // Update state
                setAdmin(adminData);

                return { success: true, data: result.data };
            } else {
                return { success: false, message: result.message || 'Login failed' };
            }
        } catch (error) {
            console.error('Login error:', error);
            return { success: false, message: 'Network error. Please try again.' };
        }
    };

    const logout = async () => {
        try {
            // SECURITY FIX: Call logout API with credentials to clear httpOnly cookie
            await fetch('https://localhost:44368/api/admin/auth/logout', {
                method: 'POST',
                credentials: 'include',  // Send httpOnly cookie with request
                headers: {
                    'Content-Type': 'application/json',
                },
            });
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            // Clear storage and state (token cookie cleared by server)
            localStorage.removeItem('admin');
            setAdmin(null);
            navigate('/admin/login');
        }
    };

    // SECURITY FIX: Token is in httpOnly cookie (not accessible to JavaScript)
    // This method is kept for backward compatibility but returns null
    const getToken = () => {
        // Token is now in httpOnly cookie (cannot be accessed by JavaScript)
        // This is a security feature to prevent XSS attacks
        return null;
    };

    const isAuthenticated = () => {
        // Check if admin data exists (token is in httpOnly cookie)
        return !!admin;
    };

    const value = {
        admin,
        loading,
        login,
        logout,
        getToken,
        isAuthenticated,
    };

    return (
        <AdminAuthContext.Provider value={value}>
            {children}
        </AdminAuthContext.Provider>
    );
};
