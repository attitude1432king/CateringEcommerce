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

    // Check for stored auth on mount
    useEffect(() => {
        const storedAdmin = localStorage.getItem('admin');
        const token = localStorage.getItem('adminToken');

        if (storedAdmin && token) {
            setAdmin(JSON.parse(storedAdmin));
        }
        setLoading(false);
    }, []);

    const login = async (username, password) => {
        try {
            const payload = {};
            payload.username = username;
            payload.password = password;
            const result = await fetchApi('/admin/auth/login', 'POST', payload)

            if (result.result && result.data) {
                const { token, ...adminData } = result.data;

                // Store in localStorage
                localStorage.setItem('adminToken', token);
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
            const token = localStorage.getItem('adminToken');

            if (token) {
                // Call logout API
                await fetch('https://localhost:44368/api/admin/auth/logout', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                    },
                });
            }
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            // Clear storage and state
            localStorage.removeItem('adminToken');
            localStorage.removeItem('admin');
            setAdmin(null);
            navigate('/admin/login');
        }
    };

    const getToken = () => {
        return localStorage.getItem('adminToken');
    };

    const isAuthenticated = () => {
        return !!admin && !!getToken();
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
