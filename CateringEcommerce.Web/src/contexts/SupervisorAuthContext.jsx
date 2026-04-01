/**
 * SupervisorAuthContext
 * Authentication and state management for Supervisor Portal.
 * SECURITY: Token is stored in an httpOnly cookie (set by backend).
 * JavaScript cannot access the token — XSS attacks cannot exfiltrate it.
 */

import {
    createContext,
    useContext,
    useState,
    useEffect,
    useCallback,
    useRef,
} from "react";
import { supervisorLogout, getSupervisorMe } from "../services/api/supervisor/supervisorAuthApi";

const SupervisorAuthContext = createContext(null);

export const useSupervisorAuth = () => {
    const context = useContext(SupervisorAuthContext);
    if (!context) {
        throw new Error(
            "useSupervisorAuth must be used within SupervisorAuthProvider"
        );
    }
    return context;
};

export const SupervisorAuthProvider = ({ children }) => {
    const [supervisor, setSupervisor] = useState(null);
    const [loading, setLoading] = useState(true);
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    const hasCheckedAuth = useRef(false);

    // 🔐 Logout — calls backend to clear httpOnly cookie, then clears local state
    const logout = useCallback(async () => {
        try {
            await supervisorLogout();
        } catch {
            // ignore — cookie will expire naturally
        } finally {
            setSupervisor(null);
            setIsAuthenticated(false);
        }
    }, []);

    // 🔍 Auth check — validates session via /api/Supervisor/auth/me (cookie auto-sent)
    const checkAuth = useCallback(async () => {
        if (hasCheckedAuth.current) return;
        hasCheckedAuth.current = true;

        try {
            const response = await getSupervisorMe();

            if (response?.success) {
                setSupervisor(response.data?.data || response.data);
                setIsAuthenticated(true);
            } else {
                setSupervisor(null);
                setIsAuthenticated(false);
            }
        } catch {
            setSupervisor(null);
            setIsAuthenticated(false);
        } finally {
            setLoading(false);
        }
    }, []);

    // 🔁 Run ONCE on mount
    useEffect(() => {
        checkAuth();
    }, [checkAuth]);

    // 🔓 Login — backend sets httpOnly cookie; we store only non-sensitive profile data
    const login = useCallback((supervisorData) => {
        setSupervisor(supervisorData);
        setIsAuthenticated(true);
    }, []);

    // 🔄 Refresh profile (safe, no auth flip)
    const refreshProfile = useCallback(async () => {
        try {
            const response = await getSupervisorMe();
            if (response?.success) {
                setSupervisor(response.data?.data || response.data);
            }
        } catch {
            // silent fail
        }
    }, []);

    // SECURITY: getToken() returns null — token is in httpOnly cookie, inaccessible to JS
    const getToken = useCallback(() => null, []);

    const value = {
        supervisor,
        loading,
        isAuthenticated,
        login,
        logout,
        refreshProfile,
        getToken,
        supervisorId:    supervisor?.supervisorId || supervisor?.id,
        supervisorType:  supervisor?.supervisorType,
        authorityLevel:  supervisor?.authorityLevel,
    };

    return (
        <SupervisorAuthContext.Provider value={value}>
            {children}
        </SupervisorAuthContext.Provider>
    );
};

export default SupervisorAuthContext;
