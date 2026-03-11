/**
 * SupervisorAuthContext
 * Authentication and state management for Supervisor Portal
 */

import {
    createContext,
    useContext,
    useState,
    useEffect,
    useCallback,
    useRef,
} from "react";
import { getProfile } from "../services/api/supervisor/supervisorApi";

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

    const hasCheckedAuth = useRef(false); // ✅ prevents double execution

    // 🔐 Logout (pure, no side effects)
    const logout = useCallback(() => {
        localStorage.removeItem("supervisorToken");
        localStorage.removeItem("supervisorId");
        setSupervisor(null);
        setIsAuthenticated(false);
    }, []);

    // 🔍 Auth check (stable + safe)
    const checkAuth = useCallback(async () => {
        if (hasCheckedAuth.current) return;
        hasCheckedAuth.current = true;

        const token = localStorage.getItem("supervisorToken");
        const supervisorId = localStorage.getItem("supervisorId");

        if (!token || !supervisorId) {
            setLoading(false);
            return;
        }

        try {
            const response = await getProfile();

            if (response?.success) {
                setSupervisor(response.data?.data || response.data);
                setIsAuthenticated(true);
            } else {
                logout();
            }
        } catch {
            logout();
        } finally {
            setLoading(false);
        }
    }, [logout]);

    // 🔁 Run ONCE on mount
    useEffect(() => {
        checkAuth();
    }, [checkAuth]);

    // 🔓 Login
    const login = useCallback((token, supervisorData) => {
        localStorage.setItem("supervisorToken", token);
        localStorage.setItem(
            "supervisorId",
            supervisorData.id || supervisorData.supervisorId
        );

        setSupervisor(supervisorData);
        setIsAuthenticated(true);
    }, []);

    // 🔄 Refresh profile (safe, no auth flip)
    const refreshProfile = useCallback(async () => {
        try {
            const response = await getProfile();
            if (response?.success) {
                setSupervisor(response.data?.data || response.data);
            }
        } catch {
            // silent fail
        }
    }, []);

    const value = {
        supervisor,
        loading,
        isAuthenticated,
        login,
        logout,
        refreshProfile,
        supervisorId:
            supervisor?.id ||
            supervisor?.supervisorId ||
            localStorage.getItem("supervisorId"),
        supervisorType: supervisor?.supervisorType,
        authorityLevel: supervisor?.authorityLevel,
    };

    return (
        <SupervisorAuthContext.Provider value={value}>
            {children}
        </SupervisorAuthContext.Provider>
    );
};

export default SupervisorAuthContext;
