import { createContext, useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchApi } from '../services/apiUtils';

const AdminAuthContext = createContext(null);

const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

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
    const [requirePasswordChange, setRequirePasswordChange] = useState(false);
    const navigate = useNavigate();

    // P0 FIX: Validate session on mount by checking httpOnly cookie with backend
    // This prevents expired/invalidated sessions from appearing as authenticated
    useEffect(() => {
        const validateSession = async () => {
            try {
                // First restore from localStorage (non-sensitive admin data)
                const storedAdmin = localStorage.getItem('admin');
                if (storedAdmin) {
                    setAdmin(JSON.parse(storedAdmin));
                }

                // Validate session with backend (httpOnly cookie sent automatically)
                if (storedAdmin) {
                    try {
                        const response = await fetchApi(`/admin/auth/me`);

                        // If response is ok, localStorage data is still valid
                        if (response && response.result) {
                            // Session valid, admin data already set above
                            return;
                        }
                    } catch (validationError) {
                        // Validation failed - cookie expired or invalid
                        console.warn('Session validation failed:', validationError);
                    }

                    // If we reach here, session is invalid
                    localStorage.removeItem('admin');
                    setAdmin(null);
                }
            } catch (error) {
                // Network error - keep stored admin for offline graceful degradation
                console.error('Session validation error:', error);
            } finally {
                setLoading(false);
            }
        };

        validateSession();
    }, []);

    const login = async (username, password) => {
        try {
            const payload = {};
            payload.username = username;
            payload.password = password;
            // SECURITY FIX: fetchApi includes credentials:'include' for httpOnly cookies
            const result = await fetchApi('/admin/auth/login', 'POST', payload);

            if (result.result && result.data) {
                // SECURITY FIX: Token no longer returned (it's in httpOnly cookie)
                // Only store non-sensitive admin data (exclude session flags like requirePasswordChange)
                const { requirePasswordChange: changeRequired, ...profileData } = result.data;

                // Store profile data (NOT token, NOT session flags) in localStorage
                localStorage.setItem('admin', JSON.stringify(profileData));

                // Update state
                setAdmin(profileData);

                // Set temporary password flag in React state only (not persisted)
                if (changeRequired) {
                    setRequirePasswordChange(true);
                }

                return { success: true, data: profileData, requirePasswordChange: !!changeRequired };
            } else {
                return { success: false, message: result.message || 'Login failed' };
            }
        } catch (error) {
            console.error('Login error:', error);
            return { success: false, message: 'Network error. Please try again.' };
        }
    };

    const clearTempFlag = () => setRequirePasswordChange(false);

    const logout = async () => {
        try {
            // SECURITY FIX: Call logout API with credentials to clear httpOnly cookie
            await fetch(`${API_BASE_URL}/api/admin/auth/logout`, {
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
        requirePasswordChange,
        clearTempFlag,
    };

    return (
        <AdminAuthContext.Provider value={value}>
            {children}
        </AdminAuthContext.Provider>
    );
};
