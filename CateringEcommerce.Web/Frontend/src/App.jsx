/*
========================================
File: src/App.jsx (UPDATED)
========================================
*/
import React, { useEffect, useRef, useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import AppHeader from './components/user/Header/AppHeader';
import AppFooter from './components/user/Footer/AppFooter';
import AuthModal from './components/user/AuthModal';

export default function App() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const location = useLocation();

    // Hide header/footer on the full-screen registration page
    const showHeaderFooter = location.pathname !== '/partner-registration';

    // Header measurement
    const headerRef = useRef(null);
    const [headerH, setHeaderH] = useState(0);

    useEffect(() => {
        if (!showHeaderFooter) {
            setHeaderH(0);
            return;
        }

        let raf = null;
        let pollInterval = null;
        let lastDPR = window.devicePixelRatio;

        const updateHeaderHeight = () => {
            if (headerRef.current) {
                const h = headerRef.current.offsetHeight || 0;
                // Only update state when value changed to avoid re-renders
                setHeaderH(prev => (prev !== h ? h : prev));
            }
        };

        const debouncedUpdate = () => {
            // debounce via rAF to avoid layout thrash
            if (raf) cancelAnimationFrame(raf);
            raf = requestAnimationFrame(() => updateHeaderHeight());
        };

        // Initial measure
        updateHeaderHeight();

        // Common triggers
        window.addEventListener('resize', debouncedUpdate);
        window.addEventListener('pageshow', debouncedUpdate);
        window.addEventListener('focus', debouncedUpdate);

        // Poll devicePixelRatio changes (some browsers don't emit events on display scale change)
        pollInterval = setInterval(() => {
            if (window.devicePixelRatio !== lastDPR) {
                lastDPR = window.devicePixelRatio;
                debouncedUpdate();
            }
        }, 350);

        // Also observe mutations that might change header height (optional, low cost)
        let observer = null;
        try {
            observer = new MutationObserver(debouncedUpdate);
            if (headerRef.current) {
                observer.observe(headerRef.current, { attributes: true, childList: true, subtree: true });
            }
        } catch (e) {
            // MutationObserver may not be available in very old environments — ignore safely
        }

        return () => {
            window.removeEventListener('resize', debouncedUpdate);
            window.removeEventListener('pageshow', debouncedUpdate);
            window.removeEventListener('focus', debouncedUpdate);
            if (raf) cancelAnimationFrame(raf);
            if (pollInterval) clearInterval(pollInterval);
            if (observer) observer.disconnect();
        };
    }, [showHeaderFooter]);

    return (
        <div className="min-h-screen flex flex-col">
            {showHeaderFooter ? (
                // attach ref to wrapper that contains the header
                <div ref={headerRef}>
                    <AppHeader onOpenAuthModal={() => setIsAuthModalOpen(true)} />
                </div>
            ) : null}

            {/* main gets padding equal to measured header height so content starts below sticky header */}
            <main className="flex-1" style={{ paddingTop: headerH }}>
                <Outlet />
            </main>

            {showHeaderFooter ? <AppFooter /> : null}

            <AuthModal isOpen={isAuthModalOpen} onClose={() => setIsAuthModalOpen(false)} />
        </div>
    );
}
